En este modulo se necesita lo siguiente:

Crear CRUD completo de ventas, donde cada endpoint necesita la autorizacion via JWT, y la estructura para crear una venta es la siguiente
los delete que aqui si se borran por completo por que puede generar conflicto en cuanto numeros, en los get vas a manejar paginacion osease page y limit

CREATE TABLE sales (
id INT PRIMARY KEY AUTO_INCREMENT,
customer VARCHAR(100) NOT NULL,
total DECIMAL(10,2) NOT NULL,
seller_id INT NOT NULL,
status VARCHAR(50) NOT NULL,
status_updated_at DATETIME NOT NULL,
status_updated_by INT NOT NULL,
created_at DATETIME NOT NULL,
updated_at DATETIME NOT NULL,
FOREIGN KEY (seller_id) REFERENCES sellers(id),
FOREIGN KEY (status_updated_by) REFERENCES sellers(id)
);

CREATE TABLE sale_items (
id INT PRIMARY KEY AUTO_INCREMENT,
sale_id INT NOT NULL,
product_id INT NOT NULL,
name VARCHAR(150) NOT NULL,
price DECIMAL(10,2) NOT NULL,
quantity INT NOT NULL,
subtotal DECIMAL(10,2) NOT NULL,
FOREIGN KEY (sale_id) REFERENCES sales(id),
FOREIGN KEY (product_id) REFERENCES products(id)
);

CREATE TABLE sale_item_extras (
sale_item_id INT NOT NULL,
extra_id INT NOT NULL,
PRIMARY KEY (sale_item_id, extra_id),
FOREIGN KEY (sale_item_id) REFERENCES sale_items(id),
FOREIGN KEY (extra_id) REFERENCES extras(id)
);
