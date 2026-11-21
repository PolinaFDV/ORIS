const cards = {
    "exploding-kitten": {
        name: "Взрывной котёнок",
        image: "images/cards/Exploding-Kitten.webp",
        type: "Особая карта",
        description: "Одна из самых опасных карт колоды.",
        effect: "Если игрок вытягивает эту карту, он должен использовать карту «Обезвредить». Иначе он выбывает из игры."
    },
    "defuse": {
        name: "Обезвредить",
        image: "images/cards/Defuse-Via-Participation-in-Kitten-Yoga.webp",
        type: "Защита",
        description: "Карта, которая помогает избежать выбывания.",
        effect: "Позволяет спастись после вытягивания взрывного котёнка."
    },
    "attack": {
        name: "Атака",
        image: "images/cards/Attack-Thousand-Year-Back-Hair.webp",
        type: "Действие",
        description: "Заставляет следующего игрока выполнять дополнительные ходы.",
        effect: "Завершает текущий ход без вытягивания карты и передаёт дополнительные ходы следующему игроку."
    }
};

const urlParams = new URLSearchParams(window.location.search);
const cardId = urlParams.get("id");
const card = cards[cardId];

if (document.getElementById("card-name")) {
    if (card) {
        document.getElementById("card-name").textContent = card.name;
        document.getElementById("card-image").src = card.image;
        document.getElementById("card-image").alt = card.name;
        document.getElementById("card-type").textContent = card.type;
        document.getElementById("card-description").textContent = card.description;
        document.getElementById("card-effect").textContent = card.effect;

        const typeSpan = document.getElementById("card-type");
        typeSpan.className = "card-type";
        if(card.type === "Особая карта") typeSpan.classList.add("danger");
        else if(card.type === "Защита") typeSpan.classList.add("safe");
        else if(card.type === "Действие") typeSpan.classList.add("action");
        else if(card.type === "Карта кота") typeSpan.classList.add("cat");

    } else {
        document.getElementById("card-name").textContent = "Карточка не найдена";
    }
}

const searchInput = document.getElementById("card-search");
const gameCards = document.querySelectorAll(".game-card");

if (searchInput) {
    searchInput.addEventListener("input", function () {
        const searchText = searchInput.value.toLowerCase().trim();

        gameCards.forEach(function (gameCard) {
            const cardName = gameCard.querySelector("h3").textContent.toLowerCase();
            
            if (cardName.includes(searchText)) {
                gameCard.style.display = "block";
            } else {
                gameCard.style.display = "none";
            }
        });
    });
}