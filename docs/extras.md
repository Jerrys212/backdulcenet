En este modulo se necesita lo siguiente:

Crear CRUD completo de extras, donde cada endpoint necesita la autorizacion via JWT, y la estructura para crear una extra es la siguiente
los delete que sean softdelete por eso el active, en los get vas a manejar paginacion osease page y limit

CREATE TABLE extras (
id INT PRIMARY KEY AUTO_INCREMENT,
name VARCHAR(100) NOT NULL,
price DECIMAL(10,2) NOT NULL,
is_active BOOLEAN NOT NULL DEFAULT TRUE,
created_at DATETIME NOT NULL,
updated_at DATETIME NOT NULL
);
