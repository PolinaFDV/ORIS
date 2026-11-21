document.addEventListener("DOMContentLoaded", function () {
    
    const loginSection = document.getElementById("login-section");
    const adminPanel = document.getElementById("admin-panel");
    const loginForm = document.getElementById("login-form");
    const logoutBtn = document.getElementById("logout-btn");

    fetch("/admin/cards")
        .then(res => {
            if (res.ok) {
                showAdminPanel();
                loadCards();
            } else {
                showLogin();
            }
        })
        .catch(() => showLogin());

    function showLogin() {
        loginSection.style.display = "block";
        adminPanel.style.display = "none";
        logoutBtn.style.display = "none";
    }

    function showAdminPanel() {
        loginSection.style.display = "none";
        adminPanel.style.display = "block";
        logoutBtn.style.display = "block";
    }

    loginForm.addEventListener("submit", function (e) {
        e.preventDefault();
        const login = document.getElementById("admin-login").value;
        const pass = document.getElementById("admin-password").value;

        fetch("/auth/login", {
            method: "POST",
            headers: { "Content-Type": "application/x-www-form-urlencoded" },
            body: new URLSearchParams({ login: login, password: pass }).toString()
        })
            .then(res => res.json())
            .then(data => {
                if (data.success) {
                    showAdminPanel();
                    loadCards();
                    document.getElementById("login-status").textContent = "";
                } else {
                    document.getElementById("login-status").textContent = data.message || "Неверный логин или пароль";
                    document.getElementById("login-status").className = "form-status error";
                }
            });
    });

    logoutBtn.addEventListener("click", function () {
        fetch("/auth/logout", { method: "POST" }).then(() => showLogin());
    });

    const quill = new Quill('#editor', {
        theme: 'snow',
        placeholder: 'Введите описание...',
        modules: { toolbar: [['bold', 'italic', 'underline'], [{ 'list': 'ordered'}, { 'list': 'bullet' }], ['clean']] }
    });

    const tableBody = document.querySelector("#cards-table tbody");
    const cardForm = document.getElementById("card-form");
    const cardStatus = document.getElementById("card-status");
    const editIdInput = document.getElementById("edit-card-id");
    const titleInput = document.getElementById("card-title");
    const kindInput = document.getElementById("card-kind");
    const imageFileInput = document.getElementById("card-image-file");
    const imageUrlInput = document.getElementById("card-image-url");
    const cancelBtn = document.getElementById("cancel-edit-btn");
    const saveBtn = document.getElementById("save-card-btn");

    imageFileInput.addEventListener("change", function () {
        const file = imageFileInput.files[0];
        if (!file) return;

        const reader = new FileReader();
        reader.onload = function (e) {
            const base64Data = e.target.result;

            
            fetch("/admin/upload-image", {
                method: "POST",
                headers: { "Content-Type": "application/x-www-form-urlencoded" },
                body: new URLSearchParams({ base64Data: base64Data }).toString()
            })
                .then(res => res.json())
                .then(data => {
                    if (data.success) {
                        imageUrlInput.value = data.url;
                        cardStatus.textContent = "Картинка загружена!";
                        cardStatus.className = "form-status success";
                        setTimeout(() => cardStatus.textContent = "", 2000);
                    } else {
                        alert("Ошибка при загрузке: " + data.message);
                    }
                });
        };
        reader.readAsDataURL(file);
    });

    function loadCards() {
        fetch("/admin/cards")
            .then(res => res.json())
            .then(cards => {
                tableBody.innerHTML = "";
                cards.forEach(card => {
                    const tr = document.createElement("tr");
                    tr.innerHTML = `
                        <td>${card.id}</td>
                        <td>${card.title}</td>
                        <td>${card.type}</td>
                        <td>
                            <button class="btn-edit" onclick="editCard(${card.id}, '${card.title}', '${card.type}', \`${card.description}\`, '${card.imageUrl || ''}')">Ред.</button>
                            <button class="btn-delete" onclick="deleteCard(${card.id})">Удал.</button>
                        </td>
                    `;
                    tableBody.appendChild(tr);
                });
            });
    }

    cardForm.addEventListener("submit", function (e) {
        e.preventDefault();

        const title = titleInput.value.trim();
        const type = kindInput.value;
        const desc = quill.root.innerHTML;
        const imageUrl = imageUrlInput.value; 

        if (!title) {
            document.getElementById("error-title").textContent = "Введите название";
            return;
        }

        const editId = editIdInput.value;
        const url = editId ? "/admin/update" : "/admin/create";

        const formData = new URLSearchParams();
        formData.append("title", title);
        formData.append("type", type);
        formData.append("description", desc);
        formData.append("imageUrl", imageUrl);
        if (editId) formData.append("id", editId);

        fetch(url, {
            method: "POST",
            headers: { "Content-Type": "application/x-www-form-urlencoded" },
            body: formData.toString()
        })
            .then(res => res.json())
            .then(data => {
                if (data.success) {
                    cardStatus.textContent = editId ? "Карта обновлена!" : "Карта добавлена!";
                    cardStatus.className = "form-status success";
                    setTimeout(() => cardStatus.textContent = "", 3000);
                    resetForm();
                    loadCards();
                }
            });
    });

    window.editCard = function (id, title, type, desc, img) {
        editIdInput.value = id;
        titleInput.value = title;
        imageUrlInput.value = img;

        for(let i=0; i<kindInput.options.length; i++){
            if(kindInput.options[i].value === type){
                kindInput.selectedIndex = i;
                break;
            }
        }

        quill.root.innerHTML = desc;
        cancelBtn.style.display = "inline-block";
        saveBtn.textContent = "Обновить карту";
        document.getElementById("error-title").textContent = "";
    };

    window.deleteCard = function (id) {
        if (confirm("Вы уверены, что хотите удалить карту?")) {
            fetch("/admin/delete", {
                method: "POST",
                headers: { "Content-Type": "application/x-www-form-urlencoded" },
                body: "id=" + id
            })
                .then(() => loadCards());
        }
    };

    cancelBtn.addEventListener("click", resetForm);

    function resetForm() {
        cardForm.reset();
        quill.root.innerHTML = "";
        editIdInput.value = "";
        imageUrlInput.value = "";
        cancelBtn.style.display = "none";
        saveBtn.textContent = "Сохранить карту";
        document.getElementById("error-title").textContent = "";
    }
});