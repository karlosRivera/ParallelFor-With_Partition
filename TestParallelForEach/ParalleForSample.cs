using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TaskPartitionExample
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            List<Person> persons = GetPerson();
            int ageTotal = 0;

            Parallel.ForEach
            (
                persons,
                () => 0,
                (person, loopState, subtotal) => subtotal + person.Age,
                (subtotal) => Interlocked.Add(ref ageTotal, subtotal)
            );


            MessageBox.Show(ageTotal.ToString());
        }

        static List<Person> GetPerson()
        {
            List<Person> p = new List<Person>
            {
                new Person() { Id = 0, Name = "Artur", Age = 5 },
                new Person() { Id = 1, Name = "Edward", Age = 10 },
                new Person() { Id = 2, Name = "Krzysiek", Age = 20 },
                new Person() { Id = 3, Name = "Piotr", Age = 15 },
                new Person() { Id = 4, Name = "Adam", Age = 10 }
            };

            return p;
        }
    }

    class Person
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
    }
}
