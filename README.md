# 🗄️ DATABASE CONNECTIONS

---

## 👤 USERS

```sql
CREATE TABLE users (
    id SERIAL PRIMARY KEY,
    username VARCHAR(32) UNIQUE NOT NULL,
    email VARCHAR(254) UNIQUE NOT NULL,
    password VARCHAR(255) NOT NULL,

    email_verified BOOLEAN NOT NULL DEFAULT FALSE,

    code VARCHAR(256) DEFAULT NULL,
    expires TIMESTAMPTZ DEFAULT NULL,

    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    password_changed_at TIMESTAMPTZ DEFAULT NULL
);
```

---

## 🛒 PRODUCTS

```sql
CREATE TABLE products (
    id SERIAL PRIMARY KEY,
    productname VARCHAR(100) UNIQUE NOT NULL,
    productstock INT NOT NULL DEFAULT 0,
    productprice DECIMAL(12, 2) NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    CONSTRAINT check_productstock
        CHECK (productstock >= 0),

    CONSTRAINT check_productprice
        CHECK (productprice >= 0)
);
```

---

## 🛍️ SHOP CART

```sql
CREATE TABLE shop_cart (
    id SERIAL PRIMARY KEY,
    userid INT NOT NULL,
    productid INT NOT NULL,
    productstock INT NOT NULL,

    CONSTRAINT fk_user_id
        FOREIGN KEY (userid) REFERENCES users(id) ON DELETE CASCADE,

    CONSTRAINT fk_product_id
        FOREIGN KEY (productid) REFERENCES products(id),

    CONSTRAINT unique_user_product
        UNIQUE (userid, productid)
);
```

---

## 🔑 REFRESH TOKENS

```sql
CREATE TABLE refreshtokens (
    id SERIAL PRIMARY KEY,
    userid INT NOT NULL,
    user_refreshtoken VARCHAR(256) NOT NULL,
    expires TIMESTAMPTZ DEFAULT NULL,

    CONSTRAINT fk_user_id
        FOREIGN KEY (userid) REFERENCES users(id) ON DELETE CASCADE
);
```

---

## 📝 Notes

- **Database:** PostgreSQL
- **Primary Keys:** `id`
- **Foreign Keys:** `userid`, `productid`
- **Authentication:** JWT Access Token + Refresh Token
- **Password Storage:** Store hashed passwords, never plaintext passwords.
- **Refresh Token Storage:** Prefer storing a hash of the refresh token rather than the raw token.
