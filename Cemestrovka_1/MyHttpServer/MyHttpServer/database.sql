DROP TABLE IF EXISTS cards;
DROP TABLE IF EXISTS feedback;
DROP TABLE IF EXISTS faq;
DROP TABLE IF EXISTS expansions;


CREATE TABLE expansions (
                            "Id" SERIAL PRIMARY KEY,
                            "Name" VARCHAR(100) NOT NULL,
                            "Description" TEXT NOT NULL,
                            "ReleaseYear" INT NOT NULL
);

INSERT INTO expansions ("Name", "Description", "ReleaseYear") VALUES
                                                                  ('Базовая игра', 'Базовый набор игры «Взрывные котята».', 2015),
                                                                  ('Imploding Kittens', 'Первое крупное дополнение, добавляющее карты «Имплодирующие котята» и новые механики.', 2016),
                                                                  ('Streaking Kittens', 'Второе дополнение с новыми картами действий и котятами.', 2018),
                                                                  ('Barking Kittens', 'Третье дополнение, добавляющее карты собак и новые правила.', 2019);

CREATE TABLE cards (
                       "Id" SERIAL PRIMARY KEY,
                       "Title" VARCHAR(100) NOT NULL,
                       "Type" VARCHAR(50) NOT NULL,
                       "Description" TEXT NOT NULL,
                       "ImageUrl" VARCHAR(255),
                       "ExpansionId" INT NOT NULL REFERENCES expansions("Id") ON DELETE CASCADE
);

INSERT INTO cards ("Title", "Type", "Description", "ImageUrl", "ExpansionId") VALUES
                                                                                  ('Взрывной котёнок', 'danger', '<p>Если игрок вытягивает эту карту и не может её обезвредить, он выбывает из игры.</p>', 'images/cards/Exploding-Kitten-House-Grenade.webp', 1),
                                                                                  ('Обезвредить', 'safe', '<p>Позволяет избежать выбывания после вытягивания взрывного котёнка. Игрок тайно возвращает котёнка в колоду.</p>', 'images/cards/Defuse-Via-Participation-in-Kitten-Yoga.webp', 1),
                                                                                  ('Атака', 'attack', '<p>Завершает ваш ход без вытягивания карты и заставляет следующего игрока сделать два хода подряд.</p>', 'images/cards/Attack-Thousand-Year-Back-Hair.webp', 1),
                                                                                  ('Пропустить ход', 'action', '<p>Позволяет завершить текущий ход без обычного вытягивания карты из колоды.</p>', 'images/cards/Skip-Don-a-Portable-Cheetah-Butt.webp', 1),
                                                                                  ('Нет-нет', 'action', '<p>Отменяет действие другой карты. Можно использовать в любой момент, даже вне своего хода.</p>', 'images/cards/Nope-A-Jackanope-Bounds-into-the-Room.webp', 1),
                                                                                  ('Одолжение', 'action', '<p>Заставляет выбранного игрока отдать вам одну карту из его руки. Какую именно карту отдать, решает этот игрок.</p>', 'images/cards/Favor-Get-Enslaved-by-Party-Squirrels.webp', 1),
                                                                                  ('Перемешать', 'action', '<p>Перемешивает колоду, не давая игрокам запомнить порядок карт.</p>', 'images/cards/Shuffle-A-Plague-of-Bat-Farts.webp', 1),
                                                                                  ('Предсказать будущее', 'action', '<p>Позволяет тайно посмотреть три верхние карты колоды, чтобы спланировать свои дальнейшие действия.</p>', 'images/cards/See-the-Future-Rub-the-Belly-of-a-Pig-a-Corn.webp', 1),
                                                                                  ('Разворот', 'action', '<p>Меняет направление хода игры.</p>', 'images/cards/Reverse-Sir-Scratch-a-Lot.webp', 1),
                                                                                  ('Такокот', 'cat', '<p>Карта кота. Может быть использована в паре с другой картой кота, чтобы украсть случайную карту у соперника.</p>', 'images/cards/Tacocat.webp', 1),
                                                                                  ('Волосатый картофелёкот', 'cat', '<p>Карта кота для составления специальных комбинаций.</p>', 'images/cards/Hairy-Potato-Cat.webp', 1),
                                                                                  ('Арбузокот', 'cat', '<p>Карта кота для составления специальных комбинаций.</p>', 'images/cards/Cattermelon.webp', 1);


CREATE TABLE feedback (
                          "Id" SERIAL PRIMARY KEY,
                          "Name" VARCHAR(50) NOT NULL,
                          "Email" VARCHAR(100) NOT NULL,
                          "Message" TEXT NOT NULL,
                          "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);


CREATE TABLE faq (
                     "Id" SERIAL PRIMARY KEY,
                     "Question" VARCHAR(255) NOT NULL,
                     "Answer" TEXT NOT NULL,
                     "Category" VARCHAR(50) NOT NULL
);

INSERT INTO faq ("Question", "Answer", "Category") VALUES
                                                       ('Можно ли использовать карту «Нет-нет» на карту «Взрывной котёнок»?', 'Нет, карта «Взрывной котёнок» не может быть отменена картой «Нет-нет».', 'Правила'),
                                                       ('Сколько карт «Обезвредить» должно быть в базовой игре?', 'В базовой игре 5 игроков и 4 карты «Обезвредить».', 'Комплектация'),
                                                       ('Что делать, если колода закончилась?', 'Перемешайте сброс и положите его в центр стола в качестве новой колоды.', 'Правила');