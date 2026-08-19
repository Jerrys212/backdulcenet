En este modulo se necesita lo siguiente:

Crear CRUD completo de productos, donde cada endpoint necesita la autorizacion via JWT, y la estructura para crear un producto es la siguiente
los delete que sean softdelete por eso el active, en los get vas a manejar paginacion osease page y limit

CREATE TABLE products (
id INT PRIMARY KEY AUTO_INCREMENT,
name VARCHAR(150) NOT NULL,
description VARCHAR(255),
category_id INT NOT NULL,
subcategory_id INT NOT NULL,
price DECIMAL(10,2) NOT NULL,
created_at DATETIME NOT NULL,
updated_at DATETIME NOT NULL,
FOREIGN KEY (category_id) REFERENCES categories(id),
FOREIGN KEY (subcategory_id) REFERENCES subcategories(id)
);
