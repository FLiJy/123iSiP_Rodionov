using System;
using System.Collections.Generic;

namespace UniversityManagementSystem
{
    // Абстрактный класс Person для абстракции общих характеристик
    public abstract class Person
    {
        private static int nextId = 1;

        // Приватные поля для инкапсуляции
        private int id;
        private string name;
        private int age;
        private string contact;
