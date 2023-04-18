

using Newtonsoft.Json;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;

namespace PlantvilleOnline
{
    public partial class MainWindow : Window, IComponentConnector
    {
        private List<Model> chats = new List<Model>();
        private List<Model> trades = new List<Model>();
        private List<Seed> seed_inventory;
        private List<Plant> garden = new List<Plant>();
        private List<Plant> inventory = new List<Plant>();
        private List<int> pending_trades = new List<int>();
        public int money = 100;
        public int land_plot = 15;
        private string username = "ceyhun";
        private static HttpClient client = new HttpClient();

        public MainWindow()
        {
            InitializeComponent();
            load_chat(lb_chat);
            if (false)
            {
                grid_sign_in.Visibility = Visibility.Hidden;
                grid_buttons.Visibility = Visibility.Visible;
                grid_chat.Visibility = Visibility.Visible;
                seed_inventory = new List<Seed>()
        {
          new Seed("strawberry", 2, 3, new TimeSpan(0, 0, 20)),
          new Seed("spinach", 5, 10, new TimeSpan(0, 0, 30)),
          new Seed("pears", 3, 6, new TimeSpan(0, 0, 60))
        };
            }
            else
                seed_inventory = new List<Seed>()
        {
          new Seed("strawberry", 2, 3, new TimeSpan(0, 0, 30)),
          new Seed("spinach", 5, 10, new TimeSpan(0, 1, 0)),
          new Seed("pears", 3, 6, new TimeSpan(0, 3, 0))
        };
            load_data();
            load_emporium();
            load_garden();
            load_plants();
        }

        private void load_plants()
        {
            foreach (Seed seed in seed_inventory)
                cb_plants.Items.Add((object)seed.Name);
        }

        private void load_data()
        {
            if (!File.Exists("data.txt"))
                return;
            using (StreamReader streamReader = new StreamReader("data.txt"))
            {
                while (!streamReader.EndOfStream)
                {
                    Dictionary<string, object> dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(streamReader.ReadLine());
                    garden = JsonConvert.DeserializeObject<List<Plant>>(dictionary["garden"].ToString());
                    inventory = JsonConvert.DeserializeObject<List<Plant>>(dictionary["inventory"].ToString());
                    money = Convert.ToInt32(dictionary["money"]);
                    pending_trades = JsonConvert.DeserializeObject<List<int>>(dictionary["pending_trades"].ToString());
                }
            }
        }

        private void btn_chat_Click(object sender, RoutedEventArgs e)
        {
            grid_chat.Visibility = Visibility.Visible;
            grid_garden.Visibility = Visibility.Hidden;
            grid_emporium.Visibility = Visibility.Hidden;
            grid_inventory.Visibility = Visibility.Hidden;
            grid_trade.Visibility = Visibility.Hidden;
            grid_trade_proposal.Visibility = Visibility.Hidden;
            load_chat(lb_chat);
        }

        private async void load_chat(ListBox lb_generic)
        {
            HttpResponseMessage response = await MainWindow.client.GetAsync("https://plantville.herokuapp.com/");
            if (!response.IsSuccessStatusCode)
                return;
            string str = await response.Content.ReadAsStringAsync();
            List<Model> chats = JsonConvert.DeserializeObject<List<Model>>(str);
            str = (string)null;
            load_lb_chat(chats, lb_generic);
            chats = (List<Model>)null;
        }

        private void load_lb_chat(List<Model> chats, ListBox lb_generic)
        {
            lb_generic.Items.Clear();
            foreach (Model chat in chats)
                lb_generic.Items.Add((object)string.Format("{0}: {1}", (object)chat.fields["username"], (object)chat.fields["message"]));
        }

        private async void btn_chat_send_Click(object sender, RoutedEventArgs e)
        {
            FormUrlEncodedContent message = new FormUrlEncodedContent((IEnumerable<KeyValuePair<string, string>>)new KeyValuePair<string, string>[2]
            {
        new KeyValuePair<string, string>("username", username),
        new KeyValuePair<string, string>("message", txt_chat.Text)
            });
            HttpResponseMessage response = await MainWindow.client.PostAsync("https://plantville.herokuapp.com/", (HttpContent)message);
            string json_data = await response.Content.ReadAsStringAsync();
            List<Model> chats = JsonConvert.DeserializeObject<List<Model>>(json_data);
            load_lb_chat(chats, lb_chat);
            txt_chat.Text = "";
        }

        private void btn_seeds_Click(object sender, RoutedEventArgs e)
        {
            grid_chat.Visibility = Visibility.Hidden;
            grid_garden.Visibility = Visibility.Hidden;
            grid_emporium.Visibility = Visibility.Visible;
            grid_inventory.Visibility = Visibility.Hidden;
            grid_trade.Visibility = Visibility.Hidden;
            grid_trade_proposal.Visibility = Visibility.Hidden;
            load_emporium();
        }

        private void load_garden()
        {
            lb_garden.Items.Clear();
            if (garden.Count < 1)
            {
                lb_garden.Items.Add((object)"No plants in garden.");
            }
            else
            {
                foreach (Plant plant in garden)
                    lb_garden.Items.Add((object)string.Format("{0} ({1})", (object)plant.Seed.Name, (object)harvestTimeLeftMessage(plant)));
            }
            update_status();
        }

        private string harvestTimeLeftMessage(Plant plant)
        {
            int num = MainWindow.HarvestTimeLeft(plant);
            if (num > 0)
                return string.Format("{0} minutes left", (object)num);
            if (num > -15)
                return "harvest";
            plant.IsSpoiled = true;
            return "spoiled";
        }

        private static int HarvestTimeLeft(Plant plant) => plant.HarvestTime.Add(plant.Seed.HarvestDuration).Subtract(DateTime.Now).Minutes;

        private void load_emporium()
        {
            lb_emporium.Items.Clear();
            foreach (Seed seed in seed_inventory)
                lb_emporium.Items.Add((object)string.Format("{0} ${1}", (object)seed.Name, (object)seed.SeedPrice));
            update_status();
        }

        private void update_status() => lbl_status.Content = (object)string.Format("Hello, {0}\n\nMoney: ${1}\nLand: {2}", (object)username, (object)money, (object)(land_plot - garden.Count));

        private void btn_garden_Click(object sender, RoutedEventArgs e)
        {
            grid_garden.Visibility = Visibility.Visible;
            grid_emporium.Visibility = Visibility.Hidden;
            grid_inventory.Visibility = Visibility.Hidden;
            grid_chat.Visibility = Visibility.Hidden;
            grid_trade.Visibility = Visibility.Hidden;
            grid_trade_proposal.Visibility = Visibility.Hidden;
            load_garden();
        }

        private void lb_emporium_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Seed seed = seed_inventory[lb_emporium.SelectedIndex];
            string messageBoxText;
            if (seed.SeedPrice > money)
                messageBoxText = "You don't have enough money.";
            else if (land_plot - garden.Count < 1)
            {
                messageBoxText = "You dont have enough land to plant another crop.";
            }
            else
            {
                money -= seed.SeedPrice;
                garden.Add(new Plant(seed));
                messageBoxText = string.Format("You purchased {0}", (object)seed.Name);
                update_status();
            }
            int num = (int)MessageBox.Show(messageBoxText);
        }

        private void btn_harvest_Click(object sender, RoutedEventArgs e)
        {
            int num1 = 0;
            foreach (Plant plant1 in garden.ToList<Plant>())
            {
                if (MainWindow.HarvestTimeLeft(plant1) <= 0 && !plant1.IsSpoiled)
                {
                    bool flag = false;
                    foreach (Plant plant2 in inventory)
                    {
                        if (plant2.Seed.Name == plant1.Seed.Name)
                        {
                            flag = true;
                            ++plant2.Quantity;
                        }
                    }
                    if (!flag)
                        inventory.Add(plant1);
                    garden.Remove(plant1);
                    ++num1;
                }
                else if (plant1.IsSpoiled)
                    garden.Remove(plant1);
            }
            if (num1 > 0)
            {
                int num2 = (int)MessageBox.Show(string.Format("Harvested {0} plants.", (object)num1));
            }
            else
            {
                int num3 = (int)MessageBox.Show("Nothing to harvest.");
            }
            load_garden();
        }

        private void btn_inventory_Click(object sender, RoutedEventArgs e)
        {
            grid_emporium.Visibility = Visibility.Hidden;
            grid_garden.Visibility = Visibility.Hidden;
            grid_inventory.Visibility = Visibility.Visible;
            grid_chat.Visibility = Visibility.Hidden;
            grid_trade.Visibility = Visibility.Hidden;
            grid_trade_proposal.Visibility = Visibility.Hidden;
            load_inventory();
        }

        private void load_inventory()
        {
            lb_inventory.Items.Clear();
            foreach (Plant plant in inventory)
                lb_inventory.Items.Add((object)string.Format("{0} [{1}] ${2}", (object)plant.Seed.Name, (object)plant.Quantity, (object)plant.Seed.SeedPrice));
            if (inventory.Count == 0)
                lb_inventory.Items.Add((object)"No fruits or vegetables harvested.");
            update_status();
        }

        private void btn_sell_Click(object sender, RoutedEventArgs e)
        {
            if (inventory.Count == 0 && MessageBox.Show("Are you sure you want to go to the farmer's market without any inventory?", "Lose money for no reason?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                return;
            int num1 = -10;
            foreach (Plant plant in inventory)
                num1 += plant.Seed.HarvestPrice;
            money += num1;
            inventory.Clear();
            int num2 = (int)MessageBox.Show(string.Format("Cleared inventory. Made ${0}", (object)num1));
            load_inventory();
        }

        private void Window_Closing(object sender, CancelEventArgs e) => File.WriteAllText("data.txt", JsonConvert.SerializeObject((object)new Dictionary<string, object>()
    {
      {
        "garden",
        (object) garden
      },
      {
        "inventory",
        (object) inventory
      },
      {
        "money",
        (object) money
      },
      {
        "pending_trades",
        (object) pending_trades
      }
    }));

        private void lb_garden_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int selectedIndex = lb_garden.SelectedIndex;
            Plant plant = garden[selectedIndex];
            int num = (int)MessageBox.Show(string.Format("{0} harvested.", (object)garden[selectedIndex].Seed.Name));
            inventory.Add(plant);
            garden.Remove(plant);
            load_garden();
        }

        private void txt_chat_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Return)
                return;
            btn_chat_send_Click(sender, (RoutedEventArgs)e);
        }

        private void btn_trade_Click(object sender, RoutedEventArgs e)
        {
            grid_garden.Visibility = Visibility.Hidden;
            grid_emporium.Visibility = Visibility.Hidden;
            grid_inventory.Visibility = Visibility.Hidden;
            grid_chat.Visibility = Visibility.Hidden;
            grid_trade.Visibility = Visibility.Visible;
            grid_trade_proposal.Visibility = Visibility.Hidden;
            load_trades();
        }

        private async void load_trades()
        {
            HttpResponseMessage response = await MainWindow.client.GetAsync("https://plantville.herokuapp.com/trades");
            if (!response.IsSuccessStatusCode)
                return;
            string str = await response.Content.ReadAsStringAsync();
            trades = JsonConvert.DeserializeObject<List<Model>>(str);
            str = (string)null;
            load_lb_trades(lb_trade);
        }

        private void load_lb_trades(ListBox lb_generic)
        {
            lb_generic.Items.Clear();
            foreach (Model trade in trades)
            {
                if (trade.fields["state"].ToString() == "open")
                    lb_generic.Items.Add((object)string.Format("[{0}] {1} wants to buy {2} {3} for ${4}", (object)trade.fields["state"], (object)trade.fields["author"], (object)trade.fields["quantity"], (object)trade.fields["plant"], (object)trade.fields["price"]));
                else
                    lb_generic.Items.Add((object)string.Format("[{0}] {1} bought {2} {3} for ${4} from {5}", (object)trade.fields["state"], (object)trade.fields["author"], (object)trade.fields["quantity"], (object)trade.fields["plant"], (object)trade.fields["price"], (object)trade.fields["accepted_by"]));
                if (pending_trades.Contains(trade.pk) && trade.fields["state"].ToString() == "closed")
                {
                    apply_trade_changes(trade, true);
                    pending_trades.Remove(trade.pk);
                }
            }
        }

        private void apply_trade_changes(Model trade, bool buying_plants)
        {
            int int32_1 = Convert.ToInt32(trade.fields["quantity"].ToString());
            int int32_2 = Convert.ToInt32(trade.fields["price"].ToString());
            Plant plant = inventory.Find((Predicate<Plant>)(x => x.Seed.Name == trade.fields["plant"]));
            if (buying_plants)
            {
                int num = (int)MessageBox.Show(string.Format("Trade accepted! You bought {0} {1} for ${2} from {3}", (object)trade.fields["quantity"], (object)trade.fields["plant"], (object)trade.fields["price"], (object)trade.fields["accepted_by"]));
                plant.Quantity += int32_1;
                money -= int32_2;
            }
            else
            {
                int num = (int)MessageBox.Show(string.Format("Trade accepted! You sold {0} {1} for ${2} to {3}", (object)trade.fields["quantity"], (object)trade.fields["plant"], (object)trade.fields["price"], (object)trade.fields["accepted_by"]));
                plant.Quantity -= int32_1;
                money += int32_2;
                if (plant.Quantity <= 0)
                    inventory.Remove(plant);
            }
            update_status();
        }

        private async void btn_trade_submit_Click(object sender, RoutedEventArgs e)
        {
            if (money < Convert.ToInt32(txt_price.Text))
            {
                int num = (int)MessageBox.Show("Error: You don't have enough money to make this trade.");
            }
            else
            {
                FormUrlEncodedContent trade = new FormUrlEncodedContent((IEnumerable<KeyValuePair<string, string>>)new KeyValuePair<string, string>[4]
                {
          new KeyValuePair<string, string>("author", username),
          new KeyValuePair<string, string>("plant", cb_plants.SelectedItem.ToString()),
          new KeyValuePair<string, string>("quantity", txt_quantity.Text),
          new KeyValuePair<string, string>("price", txt_price.Text)
                });
                HttpResponseMessage response = await MainWindow.client.PostAsync("https://plantville.herokuapp.com/trades", (HttpContent)trade);
                string trade_id = await response.Content.ReadAsStringAsync();
                pending_trades.Add(Convert.ToInt32(trade_id));
                txt_quantity.Text = "";
                txt_price.Text = "";
                cb_plants.SelectedIndex

                            = 0;
                txt_message.Visibility = Visibility.Visible;
            }
        }

        private void btn_trade_proposal_Click(object sender, RoutedEventArgs e)
        {
            grid_garden.Visibility = Visibility.Hidden;
            grid_emporium.Visibility = Visibility.Hidden;
            grid_inventory.Visibility = Visibility.Hidden;
            grid_chat.Visibility = Visibility.Hidden;
            grid_trade.Visibility = Visibility.Hidden;
            grid_trade_proposal.Visibility = Visibility.Visible;
            txt_message.Visibility = Visibility.Hidden;
        }

        private void btn_trade_accept_Click(object sender, RoutedEventArgs e)
        {
            if (lb_trade.SelectedIndex == -1)
            {
                int num1 = (int)MessageBox.Show("Error: Please select a trade to accept.");
            }
            else
            {
                Model trade = trades[lb_trade.SelectedIndex];
                if (trade.fields["author"] == username)
                {
                    int num2 = (int)MessageBox.Show("Error: You cannot accept a trade that you proposed.");
                }
                else if (trade.fields["state"] == "closed")
                {
                    int num3 = (int)MessageBox.Show("Error: Please select an open trade. You cannot accept a closed trade.");
                }
                else
                {
                    Plant plant = inventory.Find((Predicate<Plant>)(x => x.Seed.Name == trade.fields["plant"]));
                    if (plant != null)
                    {
                        if (plant.Quantity < Convert.ToInt32(trade.fields["quantity"].ToString()))
                        {
                            int num4 = (int)MessageBox.Show(string.Format("Error: You do not have enough {0} in your inventory to make trade.", (object)trade.fields["plant"].ToString()));
                        }
                        else
                        {
                            apply_trade_changes(trade, false);
                            accept_trade(trade.pk, username);
                        }
                    }
                    else
                    {
                        int num5 = (int)MessageBox.Show(string.Format("Error: You do not have {0} in your inventory.", (object)trade.fields["plant"].ToString()));
                    }
                }
            }
        }

        private async void accept_trade(int id, string username)
        {
            FormUrlEncodedContent trade = new FormUrlEncodedContent((IEnumerable<KeyValuePair<string, string>>)new KeyValuePair<string, string>[2]
            {
        new KeyValuePair<string, string>("trade_id", id.ToString()),
        new KeyValuePair<string, string>("accepted_by", username)
            });
            HttpResponseMessage response = await MainWindow.client.PostAsync("https://plantville.herokuapp.com/accept_trade", (HttpContent)trade);
            string trade_data = await response.Content.ReadAsStringAsync();
            if (trade_data == "FAIL")
            {
                int num = (int)MessageBox.Show("Error: Trade already closed. Trade not accepted.");
            }
            else
            {
                trades = JsonConvert.DeserializeObject<List<Model>>(trade_data);
                load_lb_trades(lb_trade);
            }
        }

        private void btn_sign_in_Click(object sender, RoutedEventArgs e)
        {
            username = txt_sign_in.Text;
            grid_sign_in.Visibility = Visibility.Hidden;
            grid_buttons.Visibility = Visibility.Visible;
            grid_chat.Visibility = Visibility.Visible;
            update_status();
        }

        private void txt_sign_in_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Return)
                return;
            btn_sign_in_Click(sender, (RoutedEventArgs)e);
        }

        void Connect(int connectionId, object target)
        {
            switch (connectionId)
            {
                case 1:
                    ((Window)target).Closing += new CancelEventHandler(Window_Closing);
                    break;
                case 2:
                    grid_buttons = (Grid)target;
                    break;
                case 3:
                    btn_garden = (Button)target;
                    btn_garden.Click += new RoutedEventHandler(btn_garden_Click);
                    break;
                case 4:
                    btn_seeds = (Button)target;
                    btn_seeds.Click += new RoutedEventHandler(btn_seeds_Click);
                    break;
                case 5:
                    btn_inventory = (Button)target;
                    btn_inventory.Click += new RoutedEventHandler(btn_inventory_Click);
                    break;
                case 6:
                    btn_chat = (Button)target;
                    btn_chat.Click += new RoutedEventHandler(btn_chat_Click);
                    break;
                case 7:
                    btn_trade = (Button)target;
                    btn_trade.Click += new RoutedEventHandler(btn_trade_Click);
                    break;
                case 8:
                    btn_trade_proposal = (Button)target;
                    btn_trade_proposal.Click += new RoutedEventHandler(btn_trade_proposal_Click);
                    break;
                case 9:
                    lbl_status = (Label)target;
                    break;
                case 10:
                    grid_sign_in = (Grid)target;
                    break;
                case 11:
                    txt_sign_in = (TextBox)target;
                    txt_sign_in.KeyDown += new KeyEventHandler(txt_sign_in_KeyDown);
                    break;
                case 12:
                    btn_sign_in = (Button)target;
                    btn_sign_in.Click += new RoutedEventHandler(btn_sign_in_Click);
                    break;
                case 13:
                    grid_emporium = (Grid)target;
                    break;
                case 14:
                    lb_emporium = (ListBox)target;
                    lb_emporium.MouseDoubleClick += new MouseButtonEventHandler(lb_emporium_MouseDoubleClick);
                    break;
                case 15:
                    lbl_emporium = (Label)target;
                    break;
                case 16:
                    grid_chat = (Grid)target;
                    break;
                case 17:
                    lb_chat = (ListBox)target;
                    break;
                case 18:
                    txt_chat = (TextBox)target;
                    txt_chat.KeyDown += new KeyEventHandler(txt_chat_KeyDown);
                    break;
                case 19:
                    btn_chat_send = (Button)target;
                    btn_chat_send.Click += new RoutedEventHandler(btn_chat_send_Click);
                    break;
                case 20:
                    img_chat = (Image)target;
                    break;
                case 21:
                    grid_trade = (Grid)target;
                    break;
                case 22:
                    lb_trade = (ListBox)target;
                    break;
                case 23:
                    img_trade = (Image)target;
                    break;
                case 24:
                    btn_trade_accept = (Button)target;
                    btn_trade_accept.Click += new RoutedEventHandler(btn_trade_accept_Click);
                    break;
                case 25:
                    grid_trade_proposal = (Grid)target;
                    break;
                case 26:
                    btn_trade_submit = (Button)target;
                    btn_trade_submit.Click += new RoutedEventHandler(btn_trade_submit_Click);
                    break;
                case 27:
                    txt_quantity = (TextBox)target;
                    break;
                case 28:
                    txt_price = (TextBox)target;
                    break;
                case 29:
                    cb_plants = (ComboBox)target;
                    break;
                case 30:
                    img_proposal = (Image)target;
                    break;
                case 31:
                    txt_message = (Label)target;
                    break;
                case 32:
                    grid_garden = (Grid)target;
                    break;
                case 33:
                    lb_garden = (ListBox)target;
                    lb_garden.MouseDoubleClick += new MouseButtonEventHandler(lb_garden_MouseDoubleClick);
                    break;
                case 34:
                    lbl_garden = (Label)target;
                    break;
                case 35:
                    img_garden = (Image)target;
                    break;
                case 36:
                    btn_harvest = (Button)target;
                    btn_harvest.Click += new RoutedEventHandler(btn_harvest_Click);
                    break;
                case 37:
                    grid_inventory = (Grid)target;
                    break;
                case 38:
                    lb_inventory = (ListBox)target;
                    break;
                case 39:
                    lbl_inventory = (Label)target;
                    break;
                case 40:
                    btn_sell = (Button)target;
                    btn_sell.Click += new RoutedEventHandler(btn_sell_Click);
                    break;
                default:
                    _contentLoaded = true;
                    break;
            }
        }
    }
}
