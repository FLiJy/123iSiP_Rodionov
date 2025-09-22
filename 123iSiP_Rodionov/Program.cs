//пр2  
using System;
using System.Collections.Generic;

namespace StoreInventory
{  
    public enum Category
    {
        Electronics,
        Food,
        Clothing,
        Books,
        Toys
    }
    public class Product
    {
        public int Id { get; private set; }
        public string Code => Id.ToString(); // Unique code as string representation of Id  
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public bool InStock => Quantity > 0; // Computed property for stock status  
        public Category Category { get; set; }

        public Product(int id, string name, decimal price, int quantity, Category category)
        {
            Id = id;
            Name = name;
            Price = price;
            Quantity = quantity;
            Category = category;
        }  