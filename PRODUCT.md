# Product

<!-- impeccable:product-schema 1 -->

## Platform

web

## Users

- Primary: staff of the Friendship bakery working at a desktop computer at the counter with mouse and keyboard. Five roles: cashier (BAN_HANG: retail POS, pre-orders, deposits, shift open/close), kitchen (SAN_XUAT: orders to make, batches, cake design approval), warehouse (KHO: stock in/out, stocktake, expiry alerts), manager (QUAN_LY: catalog, BOM, promotions, approvals, reports), admin (QUAN_TRI: accounts).
- Secondary: customers who do not log in, using the public `/Dat` pages to design a custom cake, pay a VietQR deposit and track it.
- Evaluator: this is a university course project (system design and analysis). The bakery is a scenario; the lecturer grading the project is a real audience.

## Product Purpose

Internal bakery management system covering use cases UC01–UC23: retail sales, pre-orders with deposit rules, kitchen workflow, ingredient stock with FEFO lots and expiry, write-offs with approval, cashier shifts, and revenue/profit reports. Success: every use case runs according to its business rules, and the interface reads as a credible commercial product.

## Positioning

A complete bakery operation in one system, where the cake a customer designs, the deposit they pay, the ingredients the kitchen draws, and the profit the manager sees are linked in a single flow. Includes the customer-facing 2D custom cake designer (Fabric.js) with a VietQR deposit.

## Operating Context

- Desktop at the counter, wide screen, mouse plus keyboard. Tablet and phone layouts are secondary and must not break.
- Vietnamese UI throughout; money formatted `1.250.000 ₫`, dates `dd/MM/yyyy HH:mm`, Vietnam time zone.
- Bank transfers are confirmed manually by the cashier from a VietQR; no bank integration. Invoices print through the browser, not thermal printers.

## Capabilities and Constraints

- Stack: ASP.NET Core Razor Pages (.NET 10), Bootstrap 5 (local `wwwroot/lib`), jQuery validation, Fabric.js for the cake designer, EF Core with SQL Server/LocalDB.
- Pages are server-rendered with antiforgery-protected forms, so form field names, routes and antiforgery behaviour must be preserved by any UI change.
- Role-based menu in `src/Friendship.Web/Auth/Menu.cs`; a direct URL outside the role shows a 403 page.
- Status is always shown with a text label, never color alone.
- Out of scope: suppliers, bank integration, customer app, thermal printing.

## Brand Commitments

- Product name is **FrendshipBakery** (replaces "Friendship" in the UI). Earlier color (caramel brown) and the 🥐 emoji logo are not binding; color and logo are chosen anew.
- Internal staff app follows the Shopify Admin convention (user-chosen reference, 2026-10-23): it should sit credibly alongside Shopify Admin, with KiotViet/Sapo as local peers.
- The public customer pages (`/Dat`) get their own separate design later; they do not have to share the staff app's look.

## Evidence on Hand

- Seeded sample data (5 accounts, 10 products with BOM, 15 ingredients with lots, vouchers, customers, orders in every state, a closed shift, write-offs, a stocktake). Password comes from config `DuLieuMau:MatKhau`.
- Manual test table UC01–UC23 in `docs/kiem-thu-tay.md`; specs and plans in `docs/superpowers/`.
- No real logo, photography, testimonials or customer data exist. Do not fabricate them.

## Product Principles

1. Business rules are visible at the point of action: deposit range, expiry state, shift status are shown where the user decides, not buried in errors.
2. The common counter task is the shortest path: selling, taking a deposit, and closing a shift come first for the cashier.
3. Each role sees only its work; navigation is shaped by role, not by the full feature list.
4. Credibility over decoration: it should be read as a product a real bakery could buy.
