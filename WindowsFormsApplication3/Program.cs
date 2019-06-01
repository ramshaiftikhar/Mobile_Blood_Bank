using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using GMap.NET;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using GMap.NET.WindowsForms.ToolTips;
using System.Collections.Specialized;
using System.Device.Location;
using System.Text;
using System.Data;
using System.IO;
namespace WindowsFormsApplication3
{
    public static class listed
    {
        public static int a = 0;
        public static int b = 0;
        public static int c = 0;
        public static string mail="";
       // public static BST tree = new BST();

    }
    public class Donor//class of donor
    {
        public int id;
        public string name;
        public string blood_group;
        public PointLatLng latlog;
        public string Email;
        public string phone_number;

        public DateTime last_donated;
        public Donor() { name = "Name"; blood_group = "Blood Group"; Email = "Email"; phone_number = "+92"; latlog = new PointLatLng(0, 0); id = 0; last_donated = new DateTime(15, 3, 12); }
        public Donor(int ids, string n, string bg, string E, String ph, double lat, double lng, int day, int month, int year)
        {
            id = ids;
            name = n;
            blood_group = bg;
            latlog = new PointLatLng(lat, lng);
            phone_number = ph;
            Email = E;
            last_donated = new DateTime(year, month, day);
        }
        public string getName()
        {
            return name;
        }
        public string getBloodGroup()
        {
            return blood_group;
        }
        public string getEmail()
        {
            return Email;
        }
        public string getPhone_number()
        {
            return phone_number;
        }
        public PointLatLng getlatlog()
        {
            return latlog;
        }
        public void setlatlog(PointLatLng a)
        {
            latlog = a;
        }
        public void setphone_number(string a)
        {
            phone_number = a;
        }
        public void setEmail(string a)
        {
            Email = a;
        }
        public void setName(string a)
        {
            name = a;
        }
        public void setBG(string a)
        {
            blood_group = a;
        }
        public void setLast_donated(DateTime a)
        {
            last_donated = a;
        }
        public TimeSpan TimePassed()
        {
            return DateTime.Now - last_donated;
        }
        public void set(int ids, string n, string bg, string E, String ph, double lat, double lng, int day, int month, int year)
        {
            id = ids;
            name = n;
            blood_group = bg;
            latlog = new PointLatLng(lat, lng);
            phone_number = ph;
            Email = E;
            last_donated = new DateTime(year, month, day);
        }



    };
    public class node
    {
        public Donor data;
        public node left;
        public node right;
    };
    public class BST
    {
        private node root;
        public int length;
        public BST()
        {
            length=0;
            root = null;
        }
        public void insert(Donor d, PointLatLng r)
        {
            
            GeoCoordinate dis =new GeoCoordinate();
            GeoCoordinate To =new GeoCoordinate();
            GeoCoordinate newN =new GeoCoordinate(d.latlog.Lat, d.latlog.Lng);
            dis.Latitude = r.Lat;
            dis.Longitude = r.Lng;
            node newnode = new node();
            newnode.data = d;
            newnode.left = null;
            newnode.right = null;
            node pp = null;
            node ptr;
            ptr = root;
            int xyz = 0;
            if (ptr == null)
            {
                if (dis.GetDistanceTo(newN) < 3000)
                {
                    root = new node();
                    root.data = newnode.data;
                    root.left = null;
                    root.right = null;
                    length++;
                }
            }
            else
            {
                while (ptr != null)
                {
                    To.Latitude = ptr.data.latlog.Lat;
                    To.Longitude = ptr.data.latlog.Lng;
                    if (dis.GetDistanceTo(newN) < 3000 && dis.GetDistanceTo(newN) < dis.GetDistanceTo(To))
                    {
                        pp = ptr;
                        ptr = ptr.left;
                        xyz = 1;
                    }
                    else if (dis.GetDistanceTo(newN) < 3000)
                    {
                        pp = ptr;
                        ptr = ptr.right;
                        xyz = 2;
                    }
                    else
                    {
                        xyz = 0;
                        break;
                    }
                }
                if (xyz == 1)
                {
                    pp.left = new node();
                    pp.left.data = newnode.data;
                    pp.left.left = null;
                    pp.left.right = null;
                    ///MessageBox.Show("left inserted" + pp.left.data.blood_group);
                    length++;
                }
                else if (xyz == 2)
                {
                    pp.right = new node();
                    pp.right.data = newnode.data;
                    pp.right.left = null;
                    pp.right.left = null;
                    ///MessageBox.Show("right inserted" + pp.right.data.blood_group);
                    length++;
                }
                else
                {

                }
            }
        }
        public Donor displaynearest()
        {
            node ptr; 
            ptr = root;
            if(ptr==null)
            {
                MessageBox.Show("No donor found in the range of 3 Kilometers!");
                return null;
            }
            if (ptr.left == null)
            {
                return ptr.data;
            }
            else
            {
                while (ptr.left != null)
                {
                    ptr = ptr.left;
                }
            }
            return ptr.data;
        }
        public node getroot()
        {
            return root;
        }
    };
    public class Tnode
    {
        public Donor data;
        public Tnode next;
    }
    public class list
    {
        public Tnode head;
        public int length;
        public list()
        {
            head = null;
            length = 0;
        }
        public void insert(string sir)
        {
            Donor d = new Donor();
            TextReader str = new StreamReader("DATA.txt");
            int[] f = new int[3];
            double cp, dp;
            string w, x, y, z;
            PointLatLng et = new PointLatLng();
            int count = 0;
            while (str.Peek() >= 0)//reading data form donor and inputting it in donor class
            {

                count++;
                d = new Donor();
                w = str.ReadLine();
                x = str.ReadLine();
                y = str.ReadLine();
                z = str.ReadLine();
                d.setName(w);
                d.setEmail(x);
                d.setphone_number(y);
                d.setBG(z);
                f[0] = Convert.ToInt32(str.ReadLine());
                f[1] = Convert.ToInt32(str.ReadLine());
                f[2] = Convert.ToInt32(str.ReadLine());
                DateTime j = new DateTime(f[2], f[1], f[0]);
                d.setLast_donated(j);
                cp = Convert.ToDouble(str.ReadLine());
                dp = Convert.ToDouble(str.ReadLine());
                et.Lat = cp; et.Lng = dp;
                d.setlatlog(et);
                if (head == null&&d.blood_group==sir)
                {
                    head = new Tnode();
                    head.data = d;
                    head.next = null;
                    length++; listed.a++;
                }
                else if(d.blood_group==sir)
                {
                    Tnode newnode = new Tnode();
                    newnode.data = new Donor();
                    newnode.data = d;
                    newnode.next = new Tnode();
                    newnode.next= head;
                    head = new Tnode();
                    head.data = newnode.data;
                    head.next = newnode.next;
                    length++; listed.a++;
                }
                
                //MessageBox.Show("inserted " + head.data.getBloodGroup());
            }
            
        }
        public int getlen()
        {
            return length;
        }
        
        public Tnode gethead()
        {
            //MessageBox.Show( head.data.blood_group+" returned");
            return head;
        }

    }
    public class Table
    {
        public LinkedList<Donor>[] L;
        public Table()
        {
            L = new LinkedList<Donor>[8];
            for(int i=0;i<8;i++)
            {
                L[i] = new LinkedList<Donor>();
            }
        }
        public void readdata()
        {
            Donor d = new Donor();
            TextReader str = new StreamReader("DATA.txt");
            int[] f = new int[3];
            double cp, dp;
            string w, x, y, z;
            PointLatLng et = new PointLatLng();
            int count = 0;
            while (str.Peek() >= 0)//reading data form donor and inputting it in donor class
            {
                count++;
                w = str.ReadLine();
                x = str.ReadLine();
                y = str.ReadLine();
                z = str.ReadLine();
                d.setName(w);
                d.setEmail(x);
                d.setphone_number(y);
                d.setBG(z);
                f[0] = Convert.ToInt32(str.ReadLine());
                f[1] = Convert.ToInt32(str.ReadLine());
                f[2] = Convert.ToInt32(str.ReadLine());
                DateTime j = new DateTime(f[2], f[1], f[0]);
                d.setLast_donated(j);
                cp = Convert.ToDouble(str.ReadLine());
                dp = Convert.ToDouble(str.ReadLine());
                et.Lat = cp; et.Lng = dp;
                d.setlatlog(et);
                string group = d.getBloodGroup();
                if (group == "A+")
                { L[0].AddFirst(d); MessageBox.Show("Inserting A+ donor");}
                else if (group == "A-")
                { L[1].AddFirst(d); MessageBox.Show("Inserting A- donor"); }
                else if (group == "B+")
                { L[2].AddFirst(d); MessageBox.Show("Inserting B+ donor"); }
                else if (group == "B-")
                { L[3].AddFirst(d); MessageBox.Show("Inserting B- donor"); }
                else if (group == "AB+")
                { L[4].AddFirst(d); MessageBox.Show("Inserting AB+ donor"); }
                else if (group == "AB-")
                { L[5].AddFirst(d); MessageBox.Show("Inserting AB- donor"); }
                else if (group == "O+")
                { L[6].AddFirst(d); MessageBox.Show("Inserting O+ donor"); }
                else if (group == "O-")
                { L[7].AddFirst(d); MessageBox.Show("Inserting O- donor"); }
            }
            str.Close();
            MessageBox.Show(count + " lines read!");

        }
        public LinkedListNode<Donor> gethead(string group)
        {
            if (group == "A+")
            {MessageBox.Show("Returning A+ donors!");
                return L[0].First; 
            }
            if (group == "A-")
            {MessageBox.Show("Returning A- donors!");
                return L[1].First; 
            }
            else if (group == "B+")
            {MessageBox.Show("Returning B+ donors!");
                return L[2].First;
            }
            else if (group == "B-")
            {MessageBox.Show("Returning B- donors!");
                return L[3].First; 
            }
            else if (group == "AB+")
            {MessageBox.Show("Returning AB+ donors!");
                return L[4].First; 
            }
            else if (group == "AB-")
            {MessageBox.Show("rReturning AB- donors!");
                return L[5].First; 
            }
            else if (group == "O+")
            {MessageBox.Show("Returning O+ donors!");
                return L[6].First; 
            }
            else if (group == "O-")
            {MessageBox.Show("Returning O- donors!");
                return L[7].First; 
            }
            else
                return null;
                
        }
        public int getlen(string group)
        {
            if (group == "A+")
                return L[0].Count;
            else if (group == "A-")
                return L[1].Count;
            else if (group == "B+")
                return L[2].Count;
            else if (group == "B-")
                return L[3].Count;
            else if (group == "AB+")
                return L[4].Count;
            else if (group == "AB-")
                return L[5].Count;
            else if (group == "O+")
                return L[6].Count;
            else if (group == "O-")
                return L[7].Count;
            else
                return 0;
            
        }


        
    };
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}