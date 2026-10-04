-- nopCommerce database initialization
CREATE DATABASE IF NOT EXISTS nopcommerce;
CREATE USER IF NOT EXISTS 'nopcommerce'@'localhost' IDENTIFIED BY 'nopCommerce_db_password';
GRANT ALL PRIVILEGES ON nopcommerce.* TO 'nopcommerce'@'localhost';
FLUSH PRIVILEGES;