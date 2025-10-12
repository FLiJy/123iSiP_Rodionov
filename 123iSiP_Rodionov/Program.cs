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
        protected Person(string name, int age, string contact)
        {
            this.id = nextId++;
            this.name = name;
            this.age = age;
            this.contact = contact;
        }

        // Публичные свойства для доступа к данным (инкапсуляция)
        public int Id => id;
        public string Name => name;
        public int Age => age;
        public string Contact => contact;

        // Абстрактный метод для полиморфизма
        public abstract string GetRole();

        // Переопределение ToString для полиморфного вывода информации
        public override string ToString()
        {
            return $"ID: {Id}, Имя: {Name}, Возраст: {Age}, Контакт: {Contact}, Роль: {GetRole()}";
        }
    }// Класс Student, наследующий от Person
    public class Student : Person
    {
        // Приватное поле для списка курсов
        private List<Course> enrolledCourses = new List<Course>();

        public Student(string name, int age, string contact) : base(name, age, contact) { }

        // Публичный метод для записи на курс
        public void Enroll(Course course)
        {
            if (!enrolledCourses.Contains(course))
            {
                enrolledCourses.Add(course);
                course.AddStudent(this);
            }
        }
        // Публичный метод для получения списка курсов
        public List<Course> GetEnrolledCourses() => new List<Course>(enrolledCourses);

        // Реализация абстрактного метода (полиморфизм)
        public override string GetRole() => "Студент";

        // Переопределение ToString с дополнительной информацией
        public override string ToString()
        {
            return base.ToString() + $", Записанные курсы: {enrolledCourses.Count}";
        }
    }
    // Класс Teacher, наследующий от Person
    public class Teacher : Person
    {
        // Приватное поле для списка курсов
        private List<Course> taughtCourses = new List<Course>();

        public Teacher(string name, int age, string contact) : base(name, age, contact) { }

        // Публичный метод для назначения на курс
        public void AssignToCourse(Course course)
        {
            if (!taughtCourses.Contains(course))
            {
                taughtCourses.Add(course);
                course.AssignTeacher(this);
            }
        }