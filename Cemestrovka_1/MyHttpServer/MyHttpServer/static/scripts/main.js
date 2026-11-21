document.addEventListener("DOMContentLoaded", function () {

    const messageInput = document.getElementById("fb-message");
    const counterDisplay = document.getElementById("fb-counter");

    if (messageInput && counterDisplay) {
        messageInput.addEventListener("input", function () {
            const length = messageInput.value.length;
            counterDisplay.textContent = length + "/500";
            if (length >= 500) {
                counterDisplay.style.color = "#ff5e54";
            } else {
                counterDisplay.style.color = "#b2b2b2";
            }
        });
    }

    const form = document.getElementById("feedback-form");
    const nameInput = document.getElementById("fb-name");
    const emailInput = document.getElementById("fb-email");

    const errorName = document.getElementById("error-name");
    const errorEmail = document.getElementById("error-email");
    const errorMessage = document.getElementById("error-message");
    const statusText = document.getElementById("fb-status");

    const nameRegex = /^[А-Яа-яЁёA-Za-z\s]{2,50}$/;
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

    function clearError(input, errorElement) {
        input.style.borderColor = "";
        errorElement.textContent = "";
    }

    function setError(input, errorElement, message) {
        input.style.borderColor = "#ff5e54";
        errorElement.textContent = message;
    }

    if (form) {
        form.addEventListener("submit", function (event) {
            event.preventDefault();

            clearError(nameInput, errorName);
            clearError(emailInput, errorEmail);
            clearError(messageInput, errorMessage);

            statusText.textContent = "";
            statusText.className = "form-status";

            let isValid = true;

            if (!nameRegex.test(nameInput.value.trim())) {
                setError(nameInput, errorName, "Введите имя (только буквы, от 2 до 50 символов)");
                isValid = false;
            }

            if (!emailRegex.test(emailInput.value.trim())) {
                setError(emailInput, errorEmail, "Введите корректный Email (например: mail@mail.ru)");
                isValid = false;
            }

            if (messageInput.value.trim().length < 10) {
                setError(messageInput, errorMessage, "Сообщение должно быть не менее 10 символов");
                isValid = false;
            }

            if (isValid) {
                const data = {
                    name: nameInput.value.trim(),
                    email: emailInput.value.trim(),
                    message: messageInput.value.trim()
                };

                fetch("/home/feedback", {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/x-www-form-urlencoded"
                    },
                    body: new URLSearchParams(data).toString()
                })
                    .then(response => {
                        if (response.ok) {
                            statusText.textContent = "Спасибо! Ваше сообщение отправлено.";
                            statusText.classList.add("success");
                            form.reset();
                            counterDisplay.textContent = "0/500";
                        } else {
                            throw new Error("Ошибка сервера");
                        }
                    })
                    .catch(error => {
                        statusText.textContent = "Произошла ошибка при отправке. Попробуйте позже.";
                        statusText.classList.add("error");
                    });
            }
        });
    }
});