-- DineFlow Korean Restaurant Seed Data
-- Menu rebuilt in Vietnamese from Korean BBQ / Hotpot menu screenshots.
-- Target schema follows the existing DineFlow EF Core migration table/column names.
-- Local/demo only.

BEGIN;

TRUNCATE TABLE
    "Payments",
    "BillDetails",
    "Bills",
    "OrderItemSelectedChoices",
    "OrderItems",
    "Orders",
    "ServiceRequests",
    "TableSessionCustomers",
    "TableSessions",
    "DiningTables",
    "ChoiceItemChannelPrices",
    "MenuItemChannelPrices",
    "MenuItemChoiceGroups",
    "ChoiceItems",
    "ChoiceGroups",
    "MenuItems",
    "MenuCategories",
    "SalesChannels",
    "Users"
RESTART IDENTITY CASCADE;

-- =========================================================
-- 1) USERS / TABLES / CHANNELS
-- =========================================================

INSERT INTO "Users" ("Username", "PasswordHash", "FullName", "Role", "IsActive", "CreatedAt")
VALUES
('admin', '$2a$11$8nEw6ozVJkjvggU2mjrQb.3f2Vnd4ojddDfw/Hr3c/HxJvkdts6/K', 'Quản trị viên', 'Admin', TRUE, NOW()),
('staff', '$2a$11$YuAvnr94mfuSPi8x8YquaekTXzNCljCrdkweAhF8SfyE5AQ2j8CAC', 'Nhân viên phục vụ', 'Staff', TRUE, NOW());

INSERT INTO "DiningTables" ("TableName", "Area", "QrToken", "Status", "IsActive", "CreatedAt")
VALUES
('Bàn 01', 'Tầng 1', 'QR-T1-001', 'Available', TRUE, NOW()),
('Bàn 02', 'Tầng 1', 'QR-T1-002', 'Available', TRUE, NOW()),
('Bàn 03', 'Tầng 1', 'QR-T1-003', 'Available', TRUE, NOW()),
('Bàn 04', 'Tầng 2', 'QR-T2-004', 'Available', TRUE, NOW()),
('Bàn VIP 01', 'Phòng VIP', 'QR-VIP-001', 'Available', TRUE, NOW());

INSERT INTO "SalesChannels" ("ChannelCode", "ChannelName", "IsActive")
VALUES
('DINE_IN', 'Tại quán', TRUE),
('CUSTOMER_WEB', 'Web khách quét QR', TRUE),
('SHOPEEFOOD', 'ShopeeFood', TRUE),
('GRABFOOD', 'GrabFood', TRUE);

-- =========================================================
-- 2) MENU CATEGORIES
-- =========================================================

INSERT INTO "MenuCategories" ("CategoryName", "Description", "DisplayOrder", "IsActive", "CreatedAt")
VALUES
('Combo', 'Combo thịt nướng kiểu Hàn, phù hợp nhóm 2-4 người.', 1, TRUE, NOW()),
('Lẩu Hàn Quốc', 'Các món lẩu kim chi, bulgogi, lòng bò, bạch tuộc và sườn bò.', 2, TRUE, NOW()),
('Thịt bò nướng', 'Các phần thịt bò cao cấp dùng để nướng tại bàn.', 3, TRUE, NOW()),
('Thịt heo nướng', 'Ba rọi, nọng, nạc dăm và ba chỉ đông lát mỏng.', 4, TRUE, NOW()),
('Nội tạng bò nướng', 'Lòng bò, ruột mỡ, trái khế, tim bò và các phần ướp.', 5, TRUE, NOW()),
('Món xào và món phụ', 'Món xào cay, bánh xèo, trứng hấp, cơm, mì và món ăn kèm.', 6, TRUE, NOW()),
('Cơm, mì và canh', 'Bibimbap, mì lạnh, canh kim chi, canh tương đậu và súp Hàn.', 7, TRUE, NOW()),
('Đồ uống', 'Nước ngọt, bia, rượu gạo, soju và đồ uống Hàn Quốc.', 8, TRUE, NOW());

-- =========================================================
-- 3) MENU ITEMS
--    Rule: BasePrice is the default dine-in price.
--    Hotpot base price = size 2-3 người. Size 4 người is configured by ChoiceGroup.
-- =========================================================

INSERT INTO "MenuItems"
("ItemCode", "CategoryId", "Name", "BasePrice", "Description", "ImageUrl", "ItemType", "IsAvailable",
 "CanOrderStandalone", "TrackStock", "AvailableQuantity", "VisibilityStatus", "Status", "DisplayOrder", "CreatedAt")
SELECT v."ItemCode", c."CategoryId", v."Name", v."BasePrice", v."Description", v."ImageUrl", v."ItemType",
       TRUE, TRUE, TRUE, v."AvailableQuantity", 'Visible', 'Active', v."DisplayOrder", NOW()
FROM (
    VALUES
    -- Combo
    ('CB-A', 'Combo', 'Combo A - Thịt bò và lòng đặc biệt', 1490000::numeric, 'Phèo, ruột mỡ, trái khế bò, tim bò, bẹ vai bò Mỹ premium, trứng hấp và canh giá đỗ kim chi. Phù hợp 3-4 người.', '/images/combo/combo-a-beef-offal.jpg', 'Combo', 12, 1),
    ('CB-B', 'Combo', 'Combo B - Lòng bò nướng', 990000::numeric, 'Phèo, ruột mỡ, trái khế bò, tim, trứng hấp và canh giá đỗ kim chi. Phù hợp 2-3 người.', '/images/combo/combo-b-offal.jpg', 'Combo', 14, 2),
    ('CB-C', 'Combo', 'Combo C - Bò cao cấp', 1390000::numeric, 'Bò Hokobe, bẹ vai bò Mỹ cao cấp, ba chỉ bò, thịt dẻ sườn bò ướp, trứng hấp và canh giá đỗ kim chi. Phù hợp 3-4 người.', '/images/combo/combo-c-premium-beef.jpg', 'Combo', 10, 3),
    ('CB-D', 'Combo', 'Combo D - Heo nướng Hàn Quốc', 590000::numeric, 'Thịt ba chỉ heo, thịt vai, nọng heo, trứng hấp và canh giá đỗ kim chi. Phù hợp 2-3 người.', '/images/combo/combo-d-pork.jpg', 'Combo', 16, 4),

    -- Hotpot
    ('LAU-01', 'Lẩu Hàn Quốc', 'Lẩu lòng bò', 550000::numeric, 'Lẩu lòng bò cay kiểu Hàn, đã bao gồm cơm. Giá mặc định cho 2-3 người.', '/images/hotpot/hotpot-beef-offal.jpg', 'Single', 15, 10),
    ('LAU-02', 'Lẩu Hàn Quốc', 'Lẩu bạch tuộc, lòng bò và tôm', 650000::numeric, 'Lẩu bạch tuộc, lòng bò và tôm, đã bao gồm cơm. Giá mặc định cho 2-3 người.', '/images/hotpot/hotpot-octopus-offal-shrimp.jpg', 'Single', 15, 11),
    ('LAU-03', 'Lẩu Hàn Quốc', 'Lẩu thịt bò sườn bò', 790000::numeric, 'Lẩu thịt bò sườn bò đậm vị, đã bao gồm cơm. Giá mặc định cho 2-3 người.', '/images/hotpot/hotpot-beef-ribs.jpg', 'Single', 12, 12),
    ('LAU-04', 'Lẩu Hàn Quốc', 'Lẩu bạch tuộc thịt bò', 590000::numeric, 'Lẩu bulgogi bạch tuộc và thịt bò, đã bao gồm cơm. Giá mặc định cho 2-3 người.', '/images/hotpot/hotpot-octopus-beef.jpg', 'Single', 14, 13),
    ('LAU-05', 'Lẩu Hàn Quốc', 'Lẩu kim chi', 370000::numeric, 'Lẩu kim chi cay nồng, đã bao gồm cơm. Giá mặc định cho 2-3 người.', '/images/hotpot/hotpot-kimchi.jpg', 'Single', 20, 14),
    ('LAU-06', 'Lẩu Hàn Quốc', 'Lẩu bulgogi', 490000::numeric, 'Lẩu bulgogi thịt bò kiểu Hàn, đã bao gồm cơm. Giá mặc định cho 2-3 người.', '/images/hotpot/hotpot-bulgogi.jpg', 'Single', 16, 15),

    -- Beef grill
    ('BEEF-01', 'Thịt bò nướng', 'Sườn bò nguyên khối premium', 890000::numeric, 'Sườn bò nguyên khối cao cấp, định lượng 500g.', '/images/beef/beef-ribs-premium.jpg', 'Single', 10, 20),
    ('BEEF-02', 'Thịt bò nướng', 'Bò Hokobe cao cấp Nhật Bản', 650000::numeric, 'Bò Hokobe nhập Nhật, định lượng 200g.', '/images/beef/beef-hokobe.jpg', 'Single', 12, 21),
    ('BEEF-03', 'Thịt bò nướng', 'Bẹ vai bò Mỹ cao cấp', 670000::numeric, 'Bẹ vai bò Mỹ cao cấp, định lượng 200g.', '/images/beef/beef-shoulder.jpg', 'Single', 12, 22),
    ('BEEF-04', 'Thịt bò nướng', 'Gầu bò Mỹ lát mỏng', 390000::numeric, 'Gầu bò Mỹ thái lát mỏng, định lượng 200g.', '/images/beef/beef-brisket-sliced.jpg', 'Single', 20, 23),
    ('BEEF-05', 'Thịt bò nướng', 'Dẻ sườn hoàng đế ướp', 390000::numeric, 'Dẻ sườn bò ướp sốt Hàn, định lượng 200g.', '/images/beef/beef-ribs-marinated.jpg', 'Single', 18, 24),
    ('BEEF-06', 'Thịt bò nướng', 'Thịt ba chỉ bò', 250000::numeric, 'Ba chỉ bò thái lát, định lượng 200g.', '/images/beef/beef-three-layers.jpg', 'Single', 25, 25),

    -- Pork grill
    ('PORK-01', 'Thịt heo nướng', 'Thịt ba rọi heo', 220000::numeric, 'Ba rọi heo nướng kiểu Hàn, định lượng 200g.', '/images/pork/pork-belly-thin-sliced.jpg', 'Single', 25, 30),
    ('PORK-02', 'Thịt heo nướng', 'Thịt nọng heo', 240000::numeric, 'Nọng heo nướng giòn béo, định lượng 200g.', '/images/pork/pork-neck-crispy.jpg', 'Single', 22, 31),
    ('PORK-03', 'Thịt heo nướng', 'Thịt nạc dăm heo', 220000::numeric, 'Nạc dăm heo mềm, phù hợp nướng tại bàn, định lượng 200g.', '/images/pork/pork-lean-tender.jpg', 'Single', 20, 32),
    ('PORK-04', 'Thịt heo nướng', 'Thịt ba chỉ đông lát mỏng', 200000::numeric, 'Ba chỉ heo đông lạnh thái lát mỏng, định lượng 200g.', '/images/pork/pork-belly-frozen-sliced.jpg', 'Single', 22, 33),

    -- Beef offal grill
    ('OFFAL-01', 'Nội tạng bò nướng', 'Lòng bò nướng', 330000::numeric, 'Lòng bò nướng kiểu Hàn, định lượng 200g.', '/images/offal/beef-small-intestine.jpg', 'Single', 18, 40),
    ('OFFAL-02', 'Nội tạng bò nướng', 'Ruột mỡ bò nướng', 330000::numeric, 'Ruột mỡ bò nướng béo giòn, định lượng 200g.', '/images/offal/beef-large-intestine.jpg', 'Single', 18, 41),
    ('OFFAL-03', 'Nội tạng bò nướng', 'Trái khế bò nướng', 340000::numeric, 'Trái khế bò nướng, định lượng 160g.', '/images/offal/beef-esophagus.jpg', 'Single', 16, 42),
    ('OFFAL-04', 'Nội tạng bò nướng', 'Lòng bò ướp', 350000::numeric, 'Lòng bò ướp sốt Hàn, định lượng 200g.', '/images/offal/beef-small-intestine-marinated.jpg', 'Single', 16, 43),
    ('OFFAL-05', 'Nội tạng bò nướng', 'Ruột mỡ bò ướp', 350000::numeric, 'Ruột mỡ bò ướp sốt Hàn, định lượng 200g.', '/images/offal/beef-large-intestine-marinated.jpg', 'Single', 16, 44),
    ('OFFAL-06', 'Nội tạng bò nướng', 'Tim bò nướng', 290000::numeric, 'Tim bò nướng, định lượng 160g.', '/images/offal/beef-heart.jpg', 'Single', 14, 45),

    -- Stir-fried and side dishes
    ('SIDE-01', 'Món xào và món phụ', 'Thịt heo xào cay', 220000::numeric, 'Thịt heo xào cay kiểu Hàn, phù hợp 2 người.', '/images/sides/pork-stir-fried-spicy.jpg', 'SideDish', 18, 50),
    ('SIDE-02', 'Món xào và món phụ', 'Bạch tuộc baby xào cay kèm mì sợi nhỏ', 340000::numeric, 'Bạch tuộc baby xào vị vừa hoặc cay, kèm mì sợi nhỏ, phù hợp 2 người.', '/images/sides/octopus-stir-fried-noodles.jpg', 'SideDish', 15, 51),
    ('SIDE-03', 'Món xào và món phụ', 'Bạch tuộc, lòng bò và tôm xào kèm mì sợi nhỏ', 450000::numeric, 'Bạch tuộc, lòng bò và tôm xào vị vừa hoặc cay, kèm mì sợi nhỏ, phù hợp 2 người.', '/images/sides/mixed-seafood-stir-fried-noodles.jpg', 'SideDish', 12, 52),
    ('SIDE-04', 'Món xào và món phụ', 'Bạch tuộc xào cay kèm mì sợi nhỏ', 390000::numeric, 'Bạch tuộc xào vị vừa hoặc cay, kèm mì sợi nhỏ, phù hợp 2 người.', '/images/sides/octopus-stir-fried-spicy-noodles.jpg', 'SideDish', 12, 53),
    ('SIDE-05', 'Món xào và món phụ', 'Bánh xèo hải sản', 220000::numeric, 'Bánh xèo hải sản kiểu Hàn.', '/images/sides/korean-seafood-pancake.jpg', 'SideDish', 18, 54),
    ('SIDE-06', 'Món xào và món phụ', 'Bánh xèo kim chi', 200000::numeric, 'Bánh xèo kim chi giòn kiểu Hàn.', '/images/sides/korean-kimchi-pancake.jpg', 'SideDish', 18, 55),
    ('SIDE-07', 'Món xào và món phụ', 'Trứng hấp', 80000::numeric, 'Trứng hấp mềm kiểu Hàn.', '/images/sides/korean-steamed-egg.jpg', 'SideDish', 25, 56),
    ('SIDE-08', 'Món xào và món phụ', 'Canh giá đỗ kim chi', 60000::numeric, 'Canh giá đỗ kim chi ăn kèm đồ nướng.', '/images/sides/korean-kimchi-sprout-soup.jpg', 'SideDish', 25, 57),
    ('SIDE-09', 'Món xào và món phụ', 'Cơm chiên', 50000::numeric, 'Cơm chiên kiểu Hàn.', '/images/sides/korean-fried-rice.jpg', 'SideDish', 30, 58),
    ('SIDE-10', 'Món xào và món phụ', 'Phần thêm mì/ramen/bún sợi nhỏ', 30000::numeric, 'Phần mì thêm dùng cho lẩu hoặc món xào.', '/images/sides/rice-noodles-extra.jpg', 'SideDish', 40, 59),
    ('SIDE-11', 'Món xào và món phụ', 'Cơm trắng', 10000::numeric, 'Một chén cơm trắng.', '/images/sides/korean-white-rice.jpg', 'SideDish', 80, 60),

    -- Rice, noodles and soup
    ('MEAL-01', 'Cơm, mì và canh', 'Bulgogi xào thố', 180000::numeric, 'Thịt bò bulgogi xào thố nóng.', '/images/meals/bulgogi-hot-stone-rice.jpg', 'Single', 18, 70),
    ('MEAL-02', 'Cơm, mì và canh', 'Súp rau thịt bò cay', 160000::numeric, 'Súp rau thịt bò cay kiểu Hàn.', '/images/meals/korean-beef-spicy-soup.jpg', 'Single', 20, 71),
    ('MEAL-03', 'Cơm, mì và canh', 'Cơm trộn thố đá', 160000::numeric, 'Bibimbap thố đá nóng với rau, trứng và tương Hàn.', '/images/meals/bibimbap-hot-stone-bowl.jpg', 'Single', 20, 72),
    ('MEAL-04', 'Cơm, mì và canh', 'Cơm trộn tô', 150000::numeric, 'Bibimbap tô truyền thống.', '/images/meals/bibimbap-traditional-bowl.jpg', 'Single', 22, 73),
    ('MEAL-05', 'Cơm, mì và canh', 'Mì lạnh nước', 150000::numeric, 'Mì lạnh nước kiểu Hàn.', '/images/meals/korean-cold-noodle-soup.jpg', 'Single', 18, 74),
    ('MEAL-06', 'Cơm, mì và canh', 'Mì lạnh trộn cay', 150000::numeric, 'Mì lạnh trộn cay kiểu Hàn.', '/images/meals/korean-cold-noodle-spicy.jpg', 'Single', 18, 75),
    ('MEAL-07', 'Cơm, mì và canh', 'Canh sườn bò', 250000::numeric, 'Canh sườn bò Hàn Quốc.', '/images/meals/korean-beef-rib-soup.jpg', 'Single', 14, 76),
    ('MEAL-08', 'Cơm, mì và canh', 'Canh kim chi', 160000::numeric, 'Canh kim chi truyền thống.', '/images/meals/korean-kimchi-soup.jpg', 'Single', 20, 77),
    ('MEAL-09', 'Cơm, mì và canh', 'Canh kim chi lòng bò', 170000::numeric, 'Canh kim chi nấu với lòng bò.', '/images/meals/korean-kimchi-offal-soup.jpg', 'Single', 18, 78),
    ('MEAL-10', 'Cơm, mì và canh', 'Canh tương đậu hải sản', 160000::numeric, 'Canh tương đậu Hàn Quốc nấu hải sản.', '/images/meals/korean-doenjang-seafood-soup.jpg', 'Single', 18, 79),
    ('MEAL-11', 'Cơm, mì và canh', 'Canh tương đậu thịt bò', 170000::numeric, 'Canh tương đậu Hàn Quốc nấu thịt bò.', '/images/meals/korean-doenjang-beef-soup.jpg', 'Single', 18, 80),
    ('MEAL-12', 'Cơm, mì và canh', 'Canh đậu hũ non hải sản', 150000::numeric, 'Canh đậu hũ non nấu hải sản.', '/images/meals/korean-soft-tofu-seafood-soup.jpg', 'Single', 18, 81),
    ('MEAL-13', 'Cơm, mì và canh', 'Canh đậu hũ non lòng bò', 160000::numeric, 'Canh đậu hũ non nấu lòng bò.', '/images/meals/korean-soft-tofu-offal-soup.jpg', 'Single', 18, 82),

    -- Drinks
    ('DRINK-01', 'Đồ uống', 'Rượu soju', 140000::numeric, 'Soju Hàn Quốc: Chamisul, Chumchurum, Jinro, Saero hoặc Goodday.', '/images/drinks/korean-soju.jpg', 'Drink', 60, 90),
    ('DRINK-02', 'Đồ uống', 'Rượu soju trái cây', 140000::numeric, 'Soju trái cây vị đào, cherry hoặc nho.', '/images/drinks/korean-soju-fruit.jpg', 'Drink', 60, 91),
    ('DRINK-03', 'Đồ uống', 'Bia lon', 40000::numeric, 'Bia Tiger, Larue hoặc Huda.', '/images/drinks/beer-can.jpg', 'Drink', 80, 92),
    ('DRINK-04', 'Đồ uống', 'Rượu gạo Hàn Quốc', 170000::numeric, 'Rượu gạo vị truyền thống, dâu, dẻ, chuối hoặc đào.', '/images/drinks/korean-rice-wine.jpg', 'Drink', 40, 93),
    ('DRINK-05', 'Đồ uống', 'Nước ngọt lon', 20000::numeric, 'Coca, Coca Zero, Sprite, Fanta cam hoặc Fanta nho.', '/images/drinks/soft-drink-can.jpg', 'Drink', 120, 94),
    ('DRINK-06', 'Đồ uống', 'Bia tươi Sapporo', 60000::numeric, 'Bia tươi Sapporo 330cc.', '/images/drinks/sapporo-draft-beer.jpg', 'Drink', 50, 95)
) AS v("ItemCode", "CategoryName", "Name", "BasePrice", "Description", "ImageUrl", "ItemType", "AvailableQuantity", "DisplayOrder")
JOIN "MenuCategories" c ON c."CategoryName" = v."CategoryName";

-- =========================================================
-- 4) CHOICE GROUPS
-- =========================================================

INSERT INTO "ChoiceGroups" ("GroupName", "DefaultMinSelect", "DefaultMaxSelect", "IsAvailable")
VALUES
('Chọn độ cay', 1, 1, TRUE),
('Chọn nước sốt chấm', 0, 2, TRUE),
('Chọn banchan miễn phí', 0, 3, TRUE),
('Thêm rau và nấm nướng', 0, 5, TRUE),
('Thêm món ăn kèm', 0, 5, TRUE),
('Thêm mì/cơm cho lẩu', 0, 3, TRUE),
('Chọn vị xào', 1, 1, TRUE),
('Chọn nước uống', 0, 4, TRUE),
('Nâng cấp combo', 0, 3, TRUE),
('Chọn khẩu phần lẩu lòng bò', 1, 1, TRUE),
('Chọn khẩu phần lẩu bạch tuộc lòng bò tôm', 1, 1, TRUE),
('Chọn khẩu phần lẩu thịt bò sườn bò', 1, 1, TRUE),
('Chọn khẩu phần lẩu bạch tuộc thịt bò', 1, 1, TRUE),
('Chọn khẩu phần lẩu kim chi', 1, 1, TRUE),
('Chọn khẩu phần lẩu bulgogi', 1, 1, TRUE);

-- =========================================================
-- 5) CHOICE ITEMS
--    Some choices are linked to real MenuItems for inventory/operation tracking.
-- =========================================================

INSERT INTO "ChoiceItems"
("ChoiceGroupId", "ChoiceName", "ExtraPrice", "LinkedMenuItemId", "IsAvailable", "DisplayOrder")
SELECT g."ChoiceGroupId", v."ChoiceName", v."ExtraPrice", linked."MenuItemId", TRUE, v."DisplayOrder"
FROM (
    VALUES
    -- Spice
    ('Chọn độ cay', 'Không cay', 0::numeric, NULL::text, 1),
    ('Chọn độ cay', 'Cay nhẹ', 0::numeric, NULL::text, 2),
    ('Chọn độ cay', 'Cay vừa', 0::numeric, NULL::text, 3),
    ('Chọn độ cay', 'Cay nhiều', 0::numeric, NULL::text, 4),
    ('Chọn độ cay', 'Siêu cay', 0::numeric, NULL::text, 5),

    -- Sauce
    ('Chọn nước sốt chấm', 'Sốt samjang Hàn Quốc', 0::numeric, NULL::text, 1),
    ('Chọn nước sốt chấm', 'Sốt dầu mè muối tiêu', 0::numeric, NULL::text, 2),
    ('Chọn nước sốt chấm', 'Sốt gochujang cay', 0::numeric, NULL::text, 3),
    ('Chọn nước sốt chấm', 'Sốt tỏi mật ong', 5000::numeric, NULL::text, 4),

    -- Banchan
    ('Chọn banchan miễn phí', 'Kim chi cải thảo', 0::numeric, NULL::text, 1),
    ('Chọn banchan miễn phí', 'Củ cải muối', 0::numeric, NULL::text, 2),
    ('Chọn banchan miễn phí', 'Salad rau trộn Hàn Quốc', 0::numeric, NULL::text, 3),
    ('Chọn banchan miễn phí', 'Giá đỗ trộn', 0::numeric, NULL::text, 4),

    -- Grill vegetables
    ('Thêm rau và nấm nướng', 'Xà lách cuốn', 15000::numeric, NULL::text, 1),
    ('Thêm rau và nấm nướng', 'Tỏi nướng', 10000::numeric, NULL::text, 2),
    ('Thêm rau và nấm nướng', 'Nấm kim châm', 20000::numeric, NULL::text, 3),
    ('Thêm rau và nấm nướng', 'Hành tây nướng', 10000::numeric, NULL::text, 4),
    ('Thêm rau và nấm nướng', 'Rau tổng hợp', 30000::numeric, NULL::text, 5),

    -- Side add-ons linked to standalone menu items
    ('Thêm món ăn kèm', 'Trứng hấp', 80000::numeric, 'Trứng hấp', 1),
    ('Thêm món ăn kèm', 'Canh giá đỗ kim chi', 60000::numeric, 'Canh giá đỗ kim chi', 2),
    ('Thêm món ăn kèm', 'Cơm chiên', 50000::numeric, 'Cơm chiên', 3),
    ('Thêm món ăn kèm', 'Cơm trắng', 10000::numeric, 'Cơm trắng', 4),
    ('Thêm món ăn kèm', 'Bánh xèo kim chi', 200000::numeric, 'Bánh xèo kim chi', 5),
    ('Thêm món ăn kèm', 'Bánh xèo hải sản', 220000::numeric, 'Bánh xèo hải sản', 6),

    -- Hotpot add-ons
    ('Thêm mì/cơm cho lẩu', 'Phần thêm mì/ramen/bún sợi nhỏ', 30000::numeric, 'Phần thêm mì/ramen/bún sợi nhỏ', 1),
    ('Thêm mì/cơm cho lẩu', 'Cơm trắng', 10000::numeric, 'Cơm trắng', 2),
    ('Thêm mì/cơm cho lẩu', 'Cơm chiên', 50000::numeric, 'Cơm chiên', 3),

    -- Stir-fried taste
    ('Chọn vị xào', 'Vị vừa', 0::numeric, NULL::text, 1),
    ('Chọn vị xào', 'Vị cay', 0::numeric, NULL::text, 2),

    -- Drinks linked to standalone drinks where possible
    ('Chọn nước uống', 'Nước ngọt lon', 20000::numeric, 'Nước ngọt lon', 1),
    ('Chọn nước uống', 'Bia lon', 40000::numeric, 'Bia lon', 2),
    ('Chọn nước uống', 'Bia tươi Sapporo', 60000::numeric, 'Bia tươi Sapporo', 3),
    ('Chọn nước uống', 'Rượu soju', 140000::numeric, 'Rượu soju', 4),
    ('Chọn nước uống', 'Rượu soju trái cây', 140000::numeric, 'Rượu soju trái cây', 5),
    ('Chọn nước uống', 'Rượu gạo Hàn Quốc', 170000::numeric, 'Rượu gạo Hàn Quốc', 6),

    -- Combo upgrades
    ('Nâng cấp combo', 'Thêm trứng hấp', 80000::numeric, 'Trứng hấp', 1),
    ('Nâng cấp combo', 'Thêm canh giá đỗ kim chi', 60000::numeric, 'Canh giá đỗ kim chi', 2),
    ('Nâng cấp combo', 'Thêm cơm trắng 2 chén', 20000::numeric, NULL::text, 3),
    ('Nâng cấp combo', 'Thêm nước ngọt 2 lon', 40000::numeric, NULL::text, 4),

    -- Hotpot portion choices. Base price is 2-3 people.
    ('Chọn khẩu phần lẩu lòng bò', '2-3 người', 0::numeric, NULL::text, 1),
    ('Chọn khẩu phần lẩu lòng bò', '4 người', 180000::numeric, NULL::text, 2),
    ('Chọn khẩu phần lẩu bạch tuộc lòng bò tôm', '2-3 người', 0::numeric, NULL::text, 1),
    ('Chọn khẩu phần lẩu bạch tuộc lòng bò tôm', '4 người', 240000::numeric, NULL::text, 2),
    ('Chọn khẩu phần lẩu thịt bò sườn bò', '2-3 người', 0::numeric, NULL::text, 1),
    ('Chọn khẩu phần lẩu thịt bò sườn bò', '4 người', 200000::numeric, NULL::text, 2),
    ('Chọn khẩu phần lẩu bạch tuộc thịt bò', '2-3 người', 0::numeric, NULL::text, 1),
    ('Chọn khẩu phần lẩu bạch tuộc thịt bò', '4 người', 200000::numeric, NULL::text, 2),
    ('Chọn khẩu phần lẩu kim chi', '2-3 người', 0::numeric, NULL::text, 1),
    ('Chọn khẩu phần lẩu kim chi', '4 người', 220000::numeric, NULL::text, 2),
    ('Chọn khẩu phần lẩu bulgogi', '2-3 người', 0::numeric, NULL::text, 1),
    ('Chọn khẩu phần lẩu bulgogi', '4 người', 200000::numeric, NULL::text, 2)
) AS v("GroupName", "ChoiceName", "ExtraPrice", "LinkedItemName", "DisplayOrder")
JOIN "ChoiceGroups" g ON g."GroupName" = v."GroupName"
LEFT JOIN "MenuItems" linked ON linked."Name" = v."LinkedItemName";

-- =========================================================
-- 6) ASSIGN CHOICE GROUPS TO MENU ITEMS
-- =========================================================

-- Combo: banchan + drink + upgrade
INSERT INTO "MenuItemChoiceGroups"
("MenuItemId", "ChoiceGroupId", "IsRequired", "MinSelect", "MaxSelect", "DisplayOrder")
SELECT mi."MenuItemId", cg."ChoiceGroupId", v."IsRequired", v."MinSelect", v."MaxSelect", v."DisplayOrder"
FROM (
    VALUES
    ('Combo A - Thịt bò và lòng đặc biệt', 'Chọn banchan miễn phí', FALSE, 0, 3, 1),
    ('Combo A - Thịt bò và lòng đặc biệt', 'Chọn nước sốt chấm', FALSE, 0, 2, 2),
    ('Combo A - Thịt bò và lòng đặc biệt', 'Chọn nước uống', FALSE, 0, 4, 3),
    ('Combo A - Thịt bò và lòng đặc biệt', 'Nâng cấp combo', FALSE, 0, 3, 4),
    ('Combo B - Lòng bò nướng', 'Chọn banchan miễn phí', FALSE, 0, 3, 1),
    ('Combo B - Lòng bò nướng', 'Chọn nước sốt chấm', FALSE, 0, 2, 2),
    ('Combo B - Lòng bò nướng', 'Chọn nước uống', FALSE, 0, 4, 3),
    ('Combo B - Lòng bò nướng', 'Nâng cấp combo', FALSE, 0, 3, 4),
    ('Combo C - Bò cao cấp', 'Chọn banchan miễn phí', FALSE, 0, 3, 1),
    ('Combo C - Bò cao cấp', 'Chọn nước sốt chấm', FALSE, 0, 2, 2),
    ('Combo C - Bò cao cấp', 'Chọn nước uống', FALSE, 0, 4, 3),
    ('Combo C - Bò cao cấp', 'Nâng cấp combo', FALSE, 0, 3, 4),
    ('Combo D - Heo nướng Hàn Quốc', 'Chọn banchan miễn phí', FALSE, 0, 3, 1),
    ('Combo D - Heo nướng Hàn Quốc', 'Chọn nước sốt chấm', FALSE, 0, 2, 2),
    ('Combo D - Heo nướng Hàn Quốc', 'Chọn nước uống', FALSE, 0, 4, 3),
    ('Combo D - Heo nướng Hàn Quốc', 'Nâng cấp combo', FALSE, 0, 3, 4)
) AS v("ItemName", "GroupName", "IsRequired", "MinSelect", "MaxSelect", "DisplayOrder")
JOIN "MenuItems" mi ON mi."Name" = v."ItemName"
JOIN "ChoiceGroups" cg ON cg."GroupName" = v."GroupName";

-- Hotpot: exact portion group + spice + noodle/rice + side + drink
INSERT INTO "MenuItemChoiceGroups"
("MenuItemId", "ChoiceGroupId", "IsRequired", "MinSelect", "MaxSelect", "DisplayOrder")
SELECT mi."MenuItemId", cg."ChoiceGroupId", v."IsRequired", v."MinSelect", v."MaxSelect", v."DisplayOrder"
FROM (
    VALUES
    ('Lẩu lòng bò', 'Chọn khẩu phần lẩu lòng bò', TRUE, 1, 1, 1),
    ('Lẩu lòng bò', 'Chọn độ cay', TRUE, 1, 1, 2),
    ('Lẩu lòng bò', 'Thêm mì/cơm cho lẩu', FALSE, 0, 3, 3),
    ('Lẩu lòng bò', 'Thêm món ăn kèm', FALSE, 0, 5, 4),
    ('Lẩu lòng bò', 'Chọn nước uống', FALSE, 0, 4, 5),

    ('Lẩu bạch tuộc, lòng bò và tôm', 'Chọn khẩu phần lẩu bạch tuộc lòng bò tôm', TRUE, 1, 1, 1),
    ('Lẩu bạch tuộc, lòng bò và tôm', 'Chọn độ cay', TRUE, 1, 1, 2),
    ('Lẩu bạch tuộc, lòng bò và tôm', 'Thêm mì/cơm cho lẩu', FALSE, 0, 3, 3),
    ('Lẩu bạch tuộc, lòng bò và tôm', 'Thêm món ăn kèm', FALSE, 0, 5, 4),
    ('Lẩu bạch tuộc, lòng bò và tôm', 'Chọn nước uống', FALSE, 0, 4, 5),

    ('Lẩu thịt bò sườn bò', 'Chọn khẩu phần lẩu thịt bò sườn bò', TRUE, 1, 1, 1),
    ('Lẩu thịt bò sườn bò', 'Chọn độ cay', TRUE, 1, 1, 2),
    ('Lẩu thịt bò sườn bò', 'Thêm mì/cơm cho lẩu', FALSE, 0, 3, 3),
    ('Lẩu thịt bò sườn bò', 'Thêm món ăn kèm', FALSE, 0, 5, 4),
    ('Lẩu thịt bò sườn bò', 'Chọn nước uống', FALSE, 0, 4, 5),

    ('Lẩu bạch tuộc thịt bò', 'Chọn khẩu phần lẩu bạch tuộc thịt bò', TRUE, 1, 1, 1),
    ('Lẩu bạch tuộc thịt bò', 'Chọn độ cay', TRUE, 1, 1, 2),
    ('Lẩu bạch tuộc thịt bò', 'Thêm mì/cơm cho lẩu', FALSE, 0, 3, 3),
    ('Lẩu bạch tuộc thịt bò', 'Thêm món ăn kèm', FALSE, 0, 5, 4),
    ('Lẩu bạch tuộc thịt bò', 'Chọn nước uống', FALSE, 0, 4, 5),

    ('Lẩu kim chi', 'Chọn khẩu phần lẩu kim chi', TRUE, 1, 1, 1),
    ('Lẩu kim chi', 'Chọn độ cay', TRUE, 1, 1, 2),
    ('Lẩu kim chi', 'Thêm mì/cơm cho lẩu', FALSE, 0, 3, 3),
    ('Lẩu kim chi', 'Thêm món ăn kèm', FALSE, 0, 5, 4),
    ('Lẩu kim chi', 'Chọn nước uống', FALSE, 0, 4, 5),

    ('Lẩu bulgogi', 'Chọn khẩu phần lẩu bulgogi', TRUE, 1, 1, 1),
    ('Lẩu bulgogi', 'Chọn độ cay', TRUE, 1, 1, 2),
    ('Lẩu bulgogi', 'Thêm mì/cơm cho lẩu', FALSE, 0, 3, 3),
    ('Lẩu bulgogi', 'Thêm món ăn kèm', FALSE, 0, 5, 4),
    ('Lẩu bulgogi', 'Chọn nước uống', FALSE, 0, 4, 5)
) AS v("ItemName", "GroupName", "IsRequired", "MinSelect", "MaxSelect", "DisplayOrder")
JOIN "MenuItems" mi ON mi."Name" = v."ItemName"
JOIN "ChoiceGroups" cg ON cg."GroupName" = v."GroupName";

-- Grill meat/offal/pork: sauce + banchan + vegetables + sides + drinks
INSERT INTO "MenuItemChoiceGroups"
("MenuItemId", "ChoiceGroupId", "IsRequired", "MinSelect", "MaxSelect", "DisplayOrder")
SELECT mi."MenuItemId", cg."ChoiceGroupId", v."IsRequired", v."MinSelect", v."MaxSelect", v."DisplayOrder"
FROM "MenuItems" mi
JOIN (
    VALUES
    ('Thịt bò nướng'),
    ('Thịt heo nướng'),
    ('Nội tạng bò nướng')
) AS cat("CategoryName") ON TRUE
JOIN "MenuCategories" mc ON mc."CategoryId" = mi."CategoryId" AND mc."CategoryName" = cat."CategoryName"
JOIN (
    VALUES
    ('Chọn nước sốt chấm', FALSE, 0, 2, 1),
    ('Chọn banchan miễn phí', FALSE, 0, 3, 2),
    ('Thêm rau và nấm nướng', FALSE, 0, 5, 3),
    ('Thêm món ăn kèm', FALSE, 0, 5, 4),
    ('Chọn nước uống', FALSE, 0, 4, 5)
) AS v("GroupName", "IsRequired", "MinSelect", "MaxSelect", "DisplayOrder") ON TRUE
JOIN "ChoiceGroups" cg ON cg."GroupName" = v."GroupName";

-- Stir-fried dishes: taste + sides + drinks
INSERT INTO "MenuItemChoiceGroups"
("MenuItemId", "ChoiceGroupId", "IsRequired", "MinSelect", "MaxSelect", "DisplayOrder")
SELECT mi."MenuItemId", cg."ChoiceGroupId", v."IsRequired", v."MinSelect", v."MaxSelect", v."DisplayOrder"
FROM (
    VALUES
    ('Thịt heo xào cay'),
    ('Bạch tuộc baby xào cay kèm mì sợi nhỏ'),
    ('Bạch tuộc, lòng bò và tôm xào kèm mì sợi nhỏ'),
    ('Bạch tuộc xào cay kèm mì sợi nhỏ')
) AS items("ItemName")
JOIN "MenuItems" mi ON mi."Name" = items."ItemName"
JOIN (
    VALUES
    ('Chọn vị xào', TRUE, 1, 1, 1),
    ('Thêm món ăn kèm', FALSE, 0, 5, 2),
    ('Chọn nước uống', FALSE, 0, 4, 3)
) AS v("GroupName", "IsRequired", "MinSelect", "MaxSelect", "DisplayOrder") ON TRUE
JOIN "ChoiceGroups" cg ON cg."GroupName" = v."GroupName";

-- Rice/noodle/soup meals: spice where relevant + sides + drinks
INSERT INTO "MenuItemChoiceGroups"
("MenuItemId", "ChoiceGroupId", "IsRequired", "MinSelect", "MaxSelect", "DisplayOrder")
SELECT mi."MenuItemId", cg."ChoiceGroupId", v."IsRequired", v."MinSelect", v."MaxSelect", v."DisplayOrder"
FROM "MenuItems" mi
JOIN "MenuCategories" mc ON mc."CategoryId" = mi."CategoryId" AND mc."CategoryName" = 'Cơm, mì và canh'
JOIN (
    VALUES
    ('Chọn độ cay', FALSE, 0, 1, 1),
    ('Thêm món ăn kèm', FALSE, 0, 5, 2),
    ('Chọn nước uống', FALSE, 0, 4, 3)
) AS v("GroupName", "IsRequired", "MinSelect", "MaxSelect", "DisplayOrder") ON TRUE
JOIN "ChoiceGroups" cg ON cg."GroupName" = v."GroupName";

-- General side dishes: drinks only, plus spice where meaningful
INSERT INTO "MenuItemChoiceGroups"
("MenuItemId", "ChoiceGroupId", "IsRequired", "MinSelect", "MaxSelect", "DisplayOrder")
SELECT mi."MenuItemId", cg."ChoiceGroupId", v."IsRequired", v."MinSelect", v."MaxSelect", v."DisplayOrder"
FROM "MenuItems" mi
JOIN "MenuCategories" mc ON mc."CategoryId" = mi."CategoryId" AND mc."CategoryName" = 'Món xào và món phụ'
JOIN (
    VALUES
    ('Chọn nước uống', FALSE, 0, 4, 1)
) AS v("GroupName", "IsRequired", "MinSelect", "MaxSelect", "DisplayOrder") ON TRUE
JOIN "ChoiceGroups" cg ON cg."GroupName" = v."GroupName"
WHERE mi."Name" NOT IN ('Thịt heo xào cay', 'Bạch tuộc baby xào cay kèm mì sợi nhỏ', 'Bạch tuộc, lòng bò và tôm xào kèm mì sợi nhỏ', 'Bạch tuộc xào cay kèm mì sợi nhỏ');

-- =========================================================
-- 7) CHANNEL PRICE CONFIGURATION
--    Delivery channels usually have commission/packaging extra price.
-- =========================================================

INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice")
SELECT mi."MenuItemId", sc."SalesChannelId", ROUND(mi."BasePrice" * v."Rate", -3)
FROM "MenuItems" mi
JOIN (
    VALUES
    ('SHOPEEFOOD', 0.12::numeric),
    ('GRABFOOD', 0.10::numeric)
) AS v("ChannelCode", "Rate") ON TRUE
JOIN "SalesChannels" sc ON sc."ChannelCode" = v."ChannelCode"
WHERE mi."ItemType" <> 'Drink';

INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice")
SELECT mi."MenuItemId", sc."SalesChannelId", v."ExtraPrice"
FROM "MenuItems" mi
JOIN (
    VALUES
    ('SHOPEEFOOD', 5000::numeric),
    ('GRABFOOD', 5000::numeric)
) AS v("ChannelCode", "ExtraPrice") ON TRUE
JOIN "SalesChannels" sc ON sc."ChannelCode" = v."ChannelCode"
WHERE mi."ItemType" = 'Drink';

INSERT INTO "ChoiceItemChannelPrices" ("ChoiceItemId", "SalesChannelId", "ChannelExtraPrice")
SELECT ci."ChoiceItemId", sc."SalesChannelId", v."ExtraPrice"
FROM (
    VALUES
    ('Thêm món ăn kèm', 'Trứng hấp', 'SHOPEEFOOD', 5000::numeric),
    ('Thêm món ăn kèm', 'Canh giá đỗ kim chi', 'SHOPEEFOOD', 3000::numeric),
    ('Thêm mì/cơm cho lẩu', 'Phần thêm mì/ramen/bún sợi nhỏ', 'SHOPEEFOOD', 3000::numeric),
    ('Chọn nước uống', 'Nước ngọt lon', 'SHOPEEFOOD', 3000::numeric),
    ('Chọn nước uống', 'Bia lon', 'SHOPEEFOOD', 5000::numeric),
    ('Thêm món ăn kèm', 'Trứng hấp', 'GRABFOOD', 4000::numeric),
    ('Thêm món ăn kèm', 'Canh giá đỗ kim chi', 'GRABFOOD', 3000::numeric),
    ('Thêm mì/cơm cho lẩu', 'Phần thêm mì/ramen/bún sợi nhỏ', 'GRABFOOD', 3000::numeric),
    ('Chọn nước uống', 'Nước ngọt lon', 'GRABFOOD', 3000::numeric),
    ('Chọn nước uống', 'Bia lon', 'GRABFOOD', 5000::numeric)
) AS v("GroupName", "ChoiceName", "ChannelCode", "ExtraPrice")
JOIN "ChoiceGroups" cg ON cg."GroupName" = v."GroupName"
JOIN "ChoiceItems" ci ON ci."ChoiceGroupId" = cg."ChoiceGroupId" AND ci."ChoiceName" = v."ChoiceName"
JOIN "SalesChannels" sc ON sc."ChannelCode" = v."ChannelCode";

COMMIT;
