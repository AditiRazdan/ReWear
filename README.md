# ReWear

ReWear is a lightweight ordering and inventory system built to support a small, sustainable thrift business.
Designed for a single local seller, the platform enables limited-stock listings, customer reservations, local pickup coordination, and simple sales analytics — helping reduce textile waste while supporting a micro-entrepreneur.

## Features
- Full customer reservation flow (listings -> reservations -> pickup confirmation -> details)
- Live listings with limited inventory and remaining stock visibility
- Quantity selector on listings before reserving
- Daily stock resets at midnight (SGT) + admin "Refresh Inventory"
- Stock enforcement at checkout
- Pickup window estimation based on recent demand (never earlier than 8:00 AM, SGT)
- Open hours messaging (8:00 AM-8:00 PM, SGT)
- Admin dashboard analytics (reservations today, revenue today)
- Top items (today + last 30 days)
- Not-selling items (last 30 days)
- Recent reservations with item breakdowns and totals
- Clear recent reservations (last 10)
- Listings management (create/edit/delete items)
- Image upload for listings
- Customer accounts (register/login/logout)
- Customer reservation history
- Printable reservation details
- Public reservation details for guest checkouts
- Role-based access (admin vs customer)
- Responsive layout with mobile tweaks
- Brand/logo integrated into navbar, receipts, and homepage
- Seed data for thrift listings with images and tags
- SQLite storage + session-based cart

## Live site
https://aditirazdan.github.io/ReWear/

## Notes
- Admin access is private
- The live site can take ~30 seconds to wake on first load.
