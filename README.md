# ReWear
ReWear is a full-stack ASP.NET Core application created to support a local sustainable thrift business, helping a small business owner increase sales through better inventory and order management.

## Sustainability & Community Impact
ReWear was built to directly support a local sustainable thrift business owner by improving how she manages inventory, orders, and customer pickups.

Before using this system, listings and orders were handled manually, making it difficult to track demand and resulting in missed sales opportunities. By centralizing listings, enforcing limited inventory, and streamlining the order flow, ReWear has helped the seller operate more efficiently and reach more customers, contributing to increased sales and more consistent turnover of secondhand items.

From an environmental perspective, supporting the resale of clothing helps extend product lifecycles and reduces demand for new garment production, which is a major contributor to textile waste and carbon emissions. The platform’s emphasis on limited-stock listings and local pickup reinforces reuse-based consumption while minimizing packaging waste and transportation impact.

This project demonstrates how targeted software solutions can meaningfully support small, sustainability-driven businesses while delivering tangible social and environmental benefits at a community scale.

## Features
- Full customer order flow (listings -> cart -> pickup confirmation -> details)
- Live listings with limited inventory and remaining stock visibility
- Quantity selector on listings before adding to cart
- Daily stock resets at midnight (SGT) + admin "Reset Daily Stock"
- Stock enforcement at checkout
- Pickup window estimation based on recent demand (never earlier than 8:00 AM, SGT)
- Open hours messaging (8:00 AM-8:00 PM, SGT)
- Admin dashboard analytics (orders today, sales today)
- Top items (today + last 30 days)
- Not-selling items (last 30 days)
- Recent orders with item breakdowns and totals
- Clear recent orders (last 10)
- Listings management (create/edit/delete items)
- Image upload for listings
- Customer accounts (register/login/logout)
- Customer order history
- Printable order details
- Public order details for guest checkouts
- Role-based access (admin vs customer)
- Responsive layout with mobile tweaks
- Brand/logo integrated into navbar, receipts, and homepage
- Seed data for thrift listings with images and tags
- SQLite storage + session-based cart

## Live site
https://aditirazdan.github.io/ReWear/

## Notes
- Admin access is private 
