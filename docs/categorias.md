En este modulo se necesita lo siguiente:

Crear CRUD completo de categorias, donde cada endpoint necesita la autorizacion via JWT, y la estructura para crear una categoria es la siguiente
los delete que sean softdelete por eso el active, en los get vas a manejar paginacion osease page y limit

CREATE TABLE categories (
id INT PRIMARY KEY AUTO_INCREMENT,
name VARCHAR(100) NOT NULL,
description VARCHAR(255),
is_active BOOLEAN NOT NULL DEFAULT TRUE,
created_at DATETIME NOT NULL,
updated_at DATETIME NOT NULL
);

CREATE TABLE subcategories (
id INT PRIMARY KEY AUTO_INCREMENT,
category_id INT NOT NULL,
name VARCHAR(100) NOT NULL,
FOREIGN KEY (category_id) REFERENCES categories(id)
);
