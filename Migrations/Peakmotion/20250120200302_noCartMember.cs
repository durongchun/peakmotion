using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace peakmotion.Migrations.Peakmotion
{
    /// <inheritdoc />
    public partial class noCartMember : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Category",
                columns: table => new
                {
                    pkcategoryid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    categorygroup = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    categoryname = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Category_pkey", x => x.pkcategoryid);
                });

            migrationBuilder.CreateTable(
                name: "Discount",
                columns: table => new
                {
                    pkdiscountid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    amount = table.Column<decimal>(type: "numeric(19)", precision: 19, nullable: false),
                    expirydate = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Discount_pkey", x => x.pkdiscountid);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    pkuserid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    lastname = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    firstname = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    phone = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    address = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    city = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    province = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    postalcode = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    country = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    email = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    usertype = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("User_pkey", x => x.pkuserid);
                });

            migrationBuilder.CreateTable(
                name: "Product",
                columns: table => new
                {
                    pkproductid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    regularprice = table.Column<decimal>(type: "numeric(19,2)", precision: 19, scale: 2, nullable: false),
                    qtyinstock = table.Column<int>(type: "integer", nullable: false),
                    isfeatured = table.Column<bool>(type: "boolean", nullable: false),
                    ismembershipproduct = table.Column<int>(type: "integer", nullable: false),
                    fkdiscountid = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Product_pkey", x => x.pkproductid);
                    table.ForeignKey(
                        name: "Product_fkdiscountid_fkey",
                        column: x => x.fkdiscountid,
                        principalTable: "Discount",
                        principalColumn: "pkdiscountid");
                });

            migrationBuilder.CreateTable(
                name: "Member",
                columns: table => new
                {
                    pkemailid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    lastloggedin = table.Column<DateOnly>(type: "date", nullable: true),
                    fkuserid = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Member_pkey", x => x.pkemailid);
                    table.ForeignKey(
                        name: "Member_fkuserid_fkey",
                        column: x => x.fkuserid,
                        principalTable: "User",
                        principalColumn: "pkuserid");
                });

            migrationBuilder.CreateTable(
                name: "Order",
                columns: table => new
                {
                    pkorderid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    orderdate = table.Column<DateOnly>(type: "date", nullable: false),
                    shippeddate = table.Column<DateOnly>(type: "date", nullable: false),
                    deliverydate = table.Column<DateOnly>(type: "date", nullable: false),
                    pptransactionid = table.Column<long>(type: "bigint", nullable: false),
                    fkuserid = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Order_pkey", x => x.pkorderid);
                    table.ForeignKey(
                        name: "Order_fkuserid_fkey",
                        column: x => x.fkuserid,
                        principalTable: "User",
                        principalColumn: "pkuserid");
                });

            migrationBuilder.CreateTable(
                name: "Product_Category",
                columns: table => new
                {
                    pkproductcategoryid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fkcategoryid = table.Column<int>(type: "integer", nullable: false),
                    fkproductid = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Product_Category_pkey", x => x.pkproductcategoryid);
                    table.ForeignKey(
                        name: "Product_Category_fkcategoryid_fkey",
                        column: x => x.fkcategoryid,
                        principalTable: "Category",
                        principalColumn: "pkcategoryid");
                    table.ForeignKey(
                        name: "Product_Category_fkproductid_fkey",
                        column: x => x.fkproductid,
                        principalTable: "Product",
                        principalColumn: "pkproductid");
                });

            migrationBuilder.CreateTable(
                name: "ProductImage",
                columns: table => new
                {
                    pkimageid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    url = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    isprimary = table.Column<bool>(type: "boolean", nullable: false),
                    alttag = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    fkproductid = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("ProductImage_pkey", x => x.pkimageid);
                    table.ForeignKey(
                        name: "ProductImage_fkproductid_fkey",
                        column: x => x.fkproductid,
                        principalTable: "Product",
                        principalColumn: "pkproductid");
                });

            migrationBuilder.CreateTable(
                name: "Review",
                columns: table => new
                {
                    pkreviewid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    comment = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    rating = table.Column<int>(type: "integer", nullable: false),
                    fkproductid = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Review_pkey", x => x.pkreviewid);
                    table.ForeignKey(
                        name: "Review_fkproductid_fkey",
                        column: x => x.fkproductid,
                        principalTable: "Product",
                        principalColumn: "pkproductid");
                });

            migrationBuilder.CreateTable(
                name: "Cart",
                columns: table => new
                {
                    pkcartid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    qty = table.Column<int>(type: "integer", nullable: false),
                    fkemailid = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Cart_pkey", x => x.pkcartid);
                    table.ForeignKey(
                        name: "Cart_fkemailid_fkey",
                        column: x => x.fkemailid,
                        principalTable: "Member",
                        principalColumn: "pkemailid");
                });

            migrationBuilder.CreateTable(
                name: "Wishlist",
                columns: table => new
                {
                    pkwishlistid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fkemailid = table.Column<int>(type: "integer", nullable: false),
                    fkproductid = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Wishlist_pkey", x => x.pkwishlistid);
                    table.ForeignKey(
                        name: "Wishlist_fkemailid_fkey",
                        column: x => x.fkemailid,
                        principalTable: "Member",
                        principalColumn: "pkemailid");
                    table.ForeignKey(
                        name: "Wishlist_fkproductid_fkey",
                        column: x => x.fkproductid,
                        principalTable: "Product",
                        principalColumn: "pkproductid");
                });

            migrationBuilder.CreateTable(
                name: "Order_Product",
                columns: table => new
                {
                    pkorderproductid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    qty = table.Column<int>(type: "integer", nullable: false),
                    unitprice = table.Column<decimal>(type: "numeric(19,2)", precision: 19, scale: 2, nullable: false),
                    fkorderid = table.Column<int>(type: "integer", nullable: false),
                    fkproductid = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Order_Product_pkey", x => x.pkorderproductid);
                    table.ForeignKey(
                        name: "Order_Product_fkorderid_fkey",
                        column: x => x.fkorderid,
                        principalTable: "Order",
                        principalColumn: "pkorderid");
                    table.ForeignKey(
                        name: "Order_Product_fkproductid_fkey",
                        column: x => x.fkproductid,
                        principalTable: "Product",
                        principalColumn: "pkproductid");
                });

            migrationBuilder.CreateTable(
                name: "OrderStatus",
                columns: table => new
                {
                    pkorderstatusid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    orderstate = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    fkoderid = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("OrderStatus_pkey", x => x.pkorderstatusid);
                    table.ForeignKey(
                        name: "OrderStatus_fkoderid_fkey",
                        column: x => x.fkoderid,
                        principalTable: "Order",
                        principalColumn: "pkorderid");
                });

            migrationBuilder.CreateTable(
                name: "Cart_Product",
                columns: table => new
                {
                    pkcartproductcid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fkcartid = table.Column<int>(type: "integer", nullable: false),
                    fkproductid = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Cart_Product_pkey", x => x.pkcartproductcid);
                    table.ForeignKey(
                        name: "Cart_Product_fkcartid_fkey",
                        column: x => x.fkcartid,
                        principalTable: "Cart",
                        principalColumn: "pkcartid");
                    table.ForeignKey(
                        name: "Cart_Product_fkproductid_fkey",
                        column: x => x.fkproductid,
                        principalTable: "Product",
                        principalColumn: "pkproductid");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cart_fkemailid",
                table: "Cart",
                column: "fkemailid");

            migrationBuilder.CreateIndex(
                name: "IX_Cart_Product_fkcartid",
                table: "Cart_Product",
                column: "fkcartid");

            migrationBuilder.CreateIndex(
                name: "IX_Cart_Product_fkproductid",
                table: "Cart_Product",
                column: "fkproductid");

            migrationBuilder.CreateIndex(
                name: "IX_Member_fkuserid",
                table: "Member",
                column: "fkuserid");

            migrationBuilder.CreateIndex(
                name: "IX_Order_fkuserid",
                table: "Order",
                column: "fkuserid");

            migrationBuilder.CreateIndex(
                name: "IX_Order_Product_fkorderid",
                table: "Order_Product",
                column: "fkorderid");

            migrationBuilder.CreateIndex(
                name: "IX_Order_Product_fkproductid",
                table: "Order_Product",
                column: "fkproductid");

            migrationBuilder.CreateIndex(
                name: "IX_OrderStatus_fkoderid",
                table: "OrderStatus",
                column: "fkoderid");

            migrationBuilder.CreateIndex(
                name: "IX_Product_fkdiscountid",
                table: "Product",
                column: "fkdiscountid");

            migrationBuilder.CreateIndex(
                name: "IX_Product_Category_fkcategoryid",
                table: "Product_Category",
                column: "fkcategoryid");

            migrationBuilder.CreateIndex(
                name: "IX_Product_Category_fkproductid",
                table: "Product_Category",
                column: "fkproductid");

            migrationBuilder.CreateIndex(
                name: "IX_ProductImage_fkproductid",
                table: "ProductImage",
                column: "fkproductid");

            migrationBuilder.CreateIndex(
                name: "IX_Review_fkproductid",
                table: "Review",
                column: "fkproductid");

            migrationBuilder.CreateIndex(
                name: "User_email_key",
                table: "User",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Wishlist_fkemailid",
                table: "Wishlist",
                column: "fkemailid");

            migrationBuilder.CreateIndex(
                name: "IX_Wishlist_fkproductid",
                table: "Wishlist",
                column: "fkproductid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Cart_Product");

            migrationBuilder.DropTable(
                name: "Order_Product");

            migrationBuilder.DropTable(
                name: "OrderStatus");

            migrationBuilder.DropTable(
                name: "Product_Category");

            migrationBuilder.DropTable(
                name: "ProductImage");

            migrationBuilder.DropTable(
                name: "Review");

            migrationBuilder.DropTable(
                name: "Wishlist");

            migrationBuilder.DropTable(
                name: "Cart");

            migrationBuilder.DropTable(
                name: "Order");

            migrationBuilder.DropTable(
                name: "Category");

            migrationBuilder.DropTable(
                name: "Product");

            migrationBuilder.DropTable(
                name: "Member");

            migrationBuilder.DropTable(
                name: "Discount");

            migrationBuilder.DropTable(
                name: "User");
        }
    }
}
