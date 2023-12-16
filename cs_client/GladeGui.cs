﻿using System;
using System.Linq;
using System.Net.Sockets;
using System.Diagnostics;
using Gtk;
using static CSharpClient;
using static Transaction;
using static User;
using static Cryptocurrency;
using static UserPortfolio;
using static UserOffer;
using static Wallet;
using static MinerUtil;
using static CustomAlertWindow;


namespace GladeFunctions
{


    public class CCTPSApp
    {

        // Socket variables

        static TcpClient client = null;


        //Global variables

        private string FILEPATH = "User/account.json";

        // Class Variables

        static User user;
        static User miningUser;
        static Wallet accountDetails;
        static UserPortfolio accountPortfolio;
        static List<Transaction> userTransactionsList;
        static UserOffer userOffer;
        static List<UserOffer> userOffersList;
        static bool checkMiner;


        // Server Assets

        private string[] currencyName =  new string[50];
        private float[] currencyPrice =  new float[50];
        private float[] currencyVolume = new float[50];
        private string[] currencyRank =   new string[50];
        private string[] currencyIcons = new string[50];

        // Buy from Server

        private int cashValue = 100;
        private int cryptoValue = 20;
        private string cryptoCurrencyName = "Bitcoin";


        // Main Window
        private bool isAscendingOrderPrice = true; // Flag to track sorting order
        private bool isAscendingOrderRank = true; // Flag to track sorting order
        private bool isAscendingOrderVolume = true; // Flag to track sorting order
        private bool isAscendingOrderName = true; // Flag to track sorting order

        private Window main_window;
        private Frame card;
        private Box market_values_box;
        private Button exchange_button_main_window;
        private EventBox rank_event_box;
        private EventBox price_event_box;
        private EventBox currency_name_event_box;
        private EventBox volume_box;
        private Button portfolio_button_gtkwindow1;
        private Button transactions_button_main_window;




        // Window 1
        private string loginEmail;
        private string loginPassword;

        private Window login_window1;
        private Entry email_entry_login_window1;
        private Entry password_entry_login_window1;
        private Button login_button_login_window1;
        private Button authorization_button_login_window1;

        // Window 2
        private string registerEmail;
        private string registerPassword;
        private string registerConfirmPassword;
        private string registerFullName;
        private string registerDateOfBirth;
        private string registerAddress;
        private string registerPhoneNumber;
        private string registerNationality;

        private Window login_window2;
        private Entry name_entry_login_window2;
        private Entry email_entry_login_window2;
        private Entry password_entry_login_window2;
        private Entry password_confirm_entry_login_window2;
        private Button back_button_login_window2;
        private Button submit_button_login_window2;
        private CheckButton agreement_login_window2;

        // Window 3
        private Window login_window3;
        private Entry phone_number_entry_login_window3;
        private Entry dob_entry_login_window3;
        private Entry address_entry_login_window3;
        private ComboBox combo_box_login_window3;
        private Entry combo_box_entry_login_window3;
        private Button submit_button_login_window3;
        private Button back_button_login_window3;

        //Window portfolio
        private Window portfolio_window;
        private Entry search_portfolio;
        private Button dashboard_button_portfolio;
        private Button p2p_button_portfolio;
        private Button portfolio_button_portfolio;
        private Button transactions_button_portfolio;
        private Button settings_button_portfolio;
        private Button help_button_porftolio;
        private Button logout_button_portfolio;

        //Window transactions

        private Window transactions_window;
        private Entry search_transactions;
        private Box transactions_box;
        private Button dashboard_button_transactions;
        private Button p2p_button_transactions;
        private Button portfolio_button_transactions;
        private Button transactions_button_transactions;
        private Button settings_button_transactions;

        private Button logout_button_transactions;

        // main Window 1
        // private Button dashboard_button_gtkwindow1;
        // private Button p2p_button_gtkwindow1;
        // private Button portfolio_button_gtkwindow1;
        // private Button settings_button_gtkwindow1;
        // private Button help_button_gtkwindow1;


        // ServerSocket functions

        private void startServerAndListenToIt(){
             // Connect to the C client (acting as a server)
            string cClientIpAddress = "127.0.0.1";
            int cClientPort = 8889;

            ConnectToCServer(cClientIpAddress, cClientPort);

            // Start a thread for listening to C server
            Thread listenerThread = new Thread(ListenToCServer);
            listenerThread.Start();
        }

        private void startServerOnThread(){
            Thread clientThread = new Thread(startServerAndListenToIt);
            clientThread.Start();
        }

        private void sendUserLoginDetails(){

            client = getClient();



            user = SetUserLoginDetails(loginEmail, loginPassword);
            user.Purpose = "GetWallet";
            string serializedUser = user.Serialize();

            // Send the serialized User object to C client
            SendMessage(client.GetStream(), serializedUser);

            // Wait for a response from the server
            Console.WriteLine("Getting response");
            accountDetails =  WaitForWallet(client.GetStream());
            Console.WriteLine("Got response");
        }
        private void sendUserRegisterDetails(){

            client = getClient();



            user = SetUserRegsitrationDetails(  registerEmail,  registerPassword,  registerFullName,  registerDateOfBirth, registerAddress,  registerPhoneNumber,  registerNationality);
            user.Purpose = "Register";
            string serializedUser = user.Serialize();

            // Send the serialized User object to C client
            SendMessage(client.GetStream(), serializedUser);

            // Wait for a response from the server
            Console.WriteLine("Getting response");

            WaitForResponse(client.GetStream());
            Console.WriteLine("Got response");
            login_window3.Hide();
            login_window1.ShowAll();
        }

        private void requestShowServerAssets(){

            int size;

            client = getClient();


            string RequestMessage = "GetServerAssetsList";

            // Send the serialized Transaction object to C client
            SendMessage(client.GetStream(), RequestMessage);

            // Wait for a response from the server
            size = WaitForServerAssets(client.GetStream(),  currencyName,  currencyPrice,  currencyVolume,  currencyRank,  currencyIcons);

            if (size == 0){

                Console.WriteLine("Size is 0");

                return;
            }

            for(int i = 0; i < size; i++){
                AddFrameToMarketValuesMainWindow(i);
            }

        }

        private void buyFromServer(){


            client = getClient();




            // Create a Transaction object and serialize it
            Transaction transaction = setTransaction("server", accountDetails.WalletAddress, cashValue, cryptoValue, cryptoCurrencyName);
            transaction.Purpose = "Publish";

            string serializedTransaction = transaction.Serialize();

            // Send the serialized Transaction object to C client
            SendMessage(client.GetStream(), serializedTransaction);

            // Wait for a response from the server
            WaitForResponse(client.GetStream());

        }

        private void requestUserPortfolio(){

            client = getClient();


            string serializedUser;

            User portfolioUser = GetUserPortfolioDetails(accountDetails.UserId);
            portfolioUser.Purpose = "GetPortfolio";
            serializedUser = portfolioUser.Serialize();

            // Send the serialized User object to C client
            SendMessage(client.GetStream(), serializedUser);

            // Wait for a response from the server
            accountPortfolio = WaitForAccountPortfolio(client.GetStream());

        }

        private void requestTransactionList(){

            client = getClient();

            Wallet userWallet = new();
            userWallet.WalletAddress = accountDetails.WalletAddress;
            userWallet.Purpose = "GetTransactionList";
            string serializedWallet = userWallet.Serialize();

            // Send the serialized User object to C client
            SendMessage(client.GetStream(), serializedWallet);

            // Wait for a response from the server
            userTransactionsList = WaitForUserTransactionList(client.GetStream());


            // Add this data to the window

            for (int i = 0; i < userTransactionsList.Count; i++){
                AddFrameToTransactionWindow(i);
            }

        }

        private void publishUserOffer(){

            client = getClient();

            userOffer = setUserOffer(accountDetails.WalletAddress, cashValue, cryptoValue, cryptoCurrencyName);
            userOffer.Purpose = "Publish";
            string serializedUserOffer = userOffer.Serialize();

            SendMessage(client.GetStream(), serializedUserOffer);

            WaitForResponse(client.GetStream());


        }

        private void requestUserOfferList(){

            client = getClient();

            string RequestMessage = "GetUserOffers";

            SendMessage(client.GetStream(), RequestMessage);

            userOffersList = WaitForUserOffers(client.GetStream());
        }

        private void checkForMiner(){

            client = getClient();

            miningUser = new();
            miningUser.Purpose = "MinerCheck";
            miningUser.UserId = accountDetails.UserId;

            string serializedUser = miningUser.Serialize();

            // Send the serialized User object to C client
            SendMessage(client.GetStream(), serializedUser);

            // Wait for a response from the server
            checkMiner = WaitForMinerCheck(client.GetStream());
        }

        private void sendMiningUserDetails(){

            client = getClient();

             // Create a User object and serialize it
            miningUser = new();
            miningUser.Purpose = "MinerRegister";
            miningUser.UserId = accountDetails.UserId;

            string serializedUser = miningUser.Serialize();


            // Send the serialized User object to C client
            SendMessage(client.GetStream(), serializedUser);

            // Wait for a response from the server
            WaitForResponse(client.GetStream());
        }




        public CCTPSApp()
        {

            // Lets connect to C server
            startServerOnThread();


            Application.Init();
            Builder builder = new Builder();
            builder.AddFromFile("GUI/Glade/CCTPS.glade");


            // CSS Declaration

            var cssProvider = new CssProvider();
            cssProvider.LoadFromPath("GUI/CSS/styles.css");

            main_window = (Window)builder.GetObject("main_window");
            main_window.DefaultSize = new Gdk.Size(1440, 968);
            login_window1 = (Window)builder.GetObject("login_window1");
            login_window1.DefaultSize = new Gdk.Size(1440, 968);
            login_window2 = (Window)builder.GetObject("login_window2");
            login_window2.DefaultSize = new Gdk.Size(1440, 968);
            login_window3 = (Window)builder.GetObject("login_window3");
            login_window3.DefaultSize = new Gdk.Size(1440, 968);
            portfolio_window = (Window)builder.GetObject("portfolio_window");
            portfolio_window.DefaultSize = new Gdk.Size(1440, 968);
            transactions_window = (Window)builder.GetObject("transactions_window");
            transactions_window.DefaultSize = new Gdk.Size(1440, 968);

            // Retrieve objects from Glade for main_window
            card = (Frame)builder.GetObject("card");
            market_values_box = (Box)builder.GetObject("market_values_box");
            exchange_button_main_window = (Button)builder.GetObject("exchange_button_main_window");
            rank_event_box = (EventBox)builder.GetObject("rank_event");
            price_event_box = (EventBox)builder.GetObject("price_event");
            currency_name_event_box = (EventBox)builder.GetObject("currency_name_event");
            volume_box = (EventBox)builder.GetObject("volume_event");
            portfolio_button_gtkwindow1 = (Button)builder.GetObject("portfolio_button_gtkwindow1");
            transactions_button_main_window = (Button)builder.GetObject("transactions_button_main_window");


            // Retrieve objects from Glade for login_window1
            email_entry_login_window1 = (Entry)builder.GetObject("email_entry_login_window1");
            password_entry_login_window1 = (Entry)builder.GetObject("password_entry_login_window1");
            login_button_login_window1 = (Button)builder.GetObject("login_button_login_window1");
            authorization_button_login_window1 = (Button)builder.GetObject("authorization_button_login_window1");

            // Retrieve objects from Glade for login_window2
            name_entry_login_window2 = (Entry)builder.GetObject("name_entry_login_window2");
            email_entry_login_window2 = (Entry)builder.GetObject("email_entry_login_window2");
            password_entry_login_window2 = (Entry)builder.GetObject("password_entry_login_window2");
            password_confirm_entry_login_window2 = (Entry)builder.GetObject("password_confirm_entry_login_window2");
            agreement_login_window2 = (CheckButton)builder.GetObject("agreement_login_window2");
            submit_button_login_window2 = (Button)builder.GetObject("submit_button_login_window2");
            back_button_login_window2 = (Button)builder.GetObject("back_button_login_window2");

            // Window 3
            phone_number_entry_login_window3 = (Entry)builder.GetObject("phone_number_entry_login_window3");
            dob_entry_login_window3 = (Entry)builder.GetObject("dob_entry_login_window3");
            dob_entry_login_window3.MaxLength = 10;
            combo_box_login_window3 = (ComboBox)builder.GetObject("combo_box_login_window3");
            combo_box_entry_login_window3 = combo_box_login_window3.Child as Entry;

            address_entry_login_window3 = (Entry)builder.GetObject("address_entry_login_window3");
            submit_button_login_window3 = (Button)builder.GetObject("submit_button_login_window3");
            back_button_login_window3 = (Button)builder.GetObject("back_button_login_window3");


            //Window portfolio
            dashboard_button_portfolio = (Button)builder.GetObject("dashboard_button_portfolio");
            p2p_button_portfolio = (Button)builder.GetObject("p2p_button_portfolio");
            portfolio_button_portfolio = (Button)builder.GetObject("portfolio_button_portfolio");
            transactions_button_portfolio = (Button)builder.GetObject("transactions_button_portfolio");
            settings_button_portfolio = (Button)builder.GetObject("settings_button_portfolio");
            help_button_porftolio = (Button)builder.GetObject("help_button_porftolio");
            logout_button_portfolio = (Button)builder.GetObject("logout_button_portfolio");
            search_portfolio = (Entry)builder.GetObject("search_portfolio");

            // Window transactions

            transactions_box = (Box)builder.GetObject("transactions_box");


            // Main Window
            portfolio_button_portfolio.Clicked += portfolio_button_gtkwindow1_clicked;
            transactions_button_main_window.Clicked += transactions_button_main_window_clicked;

            // Connect button click events for login_window1
            login_button_login_window1.Clicked += login_button_login_window1_clicked;
            authorization_button_login_window1.Clicked += authorization_button_login_window1_clicked;

            // Connect button click events for login_window2
            submit_button_login_window2.Clicked += submit_button_login_window2_clicked;
            back_button_login_window2.Clicked += back_button_login_window2_clicked;
            submit_button_login_window3.Clicked += submit_button_login_window3_clicked;

            // Connect toggle event for agreement CheckButton
            agreement_login_window2.Toggled += agreement_login_window2_toggled;

            //Window 3
            back_button_login_window3.Clicked += back_button_login_window3_clicked;
            dob_entry_login_window3.Changed += OnEntryChanged;

            // Main window Sorting
            price_event_box.ButtonPressEvent += OnSortButtonPrice;
            rank_event_box.ButtonPressEvent += OnSortButtonRank;
            currency_name_event_box.ButtonPressEvent += OnSortCurrencyName;
            volume_box.ButtonPressEvent +=OnSortVolume;



            // CSS Button

            var login_button_login_window1_css = login_button_login_window1.StyleContext;
            login_button_login_window1_css.AddProvider(cssProvider, Gtk.StyleProviderPriority.Application);
            login_button_login_window1_css.AddClass("login-button-login-window1");

            var authorization_button_login_window1_css = authorization_button_login_window1.StyleContext;
            authorization_button_login_window1_css.AddProvider(cssProvider, Gtk.StyleProviderPriority.Application);
            authorization_button_login_window1_css.AddClass("authorization-button-login-window1");

            var submit_button_login_window2_css = submit_button_login_window2.StyleContext;
            submit_button_login_window2_css.AddProvider(cssProvider, Gtk.StyleProviderPriority.Application);
            submit_button_login_window2_css.AddClass("submit-button-login-window2");

            var submit_button_login_window3_css = submit_button_login_window3.StyleContext;
            submit_button_login_window3_css.AddProvider(cssProvider, Gtk.StyleProviderPriority.Application);
            submit_button_login_window3_css.AddClass("submit-button-login-window3");

            var back_button_login_window3_css = back_button_login_window3.StyleContext;
            back_button_login_window3_css.AddProvider(cssProvider, Gtk.StyleProviderPriority.Application);
            back_button_login_window3_css.AddClass("back-button-login-window3");

            var dashboard_button_portfolio_css = dashboard_button_portfolio.StyleContext;
            dashboard_button_portfolio_css.AddProvider(cssProvider, Gtk.StyleProviderPriority.Application);
            dashboard_button_portfolio_css.AddClass("dashboard-button-portfolio");

            var p2p_button_portfolio_css = p2p_button_portfolio.StyleContext;
            p2p_button_portfolio_css.AddProvider(cssProvider, Gtk.StyleProviderPriority.Application);
            p2p_button_portfolio_css.AddClass("p2p-button-portfolio");

            var portfolio_button_portfolio_css = portfolio_button_portfolio.StyleContext;
            portfolio_button_portfolio_css.AddProvider(cssProvider, Gtk.StyleProviderPriority.Application);
            portfolio_button_portfolio_css.AddClass("portfolio-button-portfolio");

            var transactions_button_portfolio_css = transactions_button_portfolio.StyleContext;
            transactions_button_portfolio_css.AddProvider(cssProvider, Gtk.StyleProviderPriority.Application);
            transactions_button_portfolio_css.AddClass("transactions-button-portfolio");

            var settings_button_portfolio_css = settings_button_portfolio.StyleContext;
            settings_button_portfolio_css.AddProvider(cssProvider, Gtk.StyleProviderPriority.Application);
            settings_button_portfolio_css.AddClass("settings-button-portfolio");

            var help_button_porftolio_css = help_button_porftolio.StyleContext;
            help_button_porftolio_css.AddProvider(cssProvider, Gtk.StyleProviderPriority.Application);
            help_button_porftolio_css.AddClass("help-button-porftolio");

            var logout_button_portfolio_css = logout_button_portfolio.StyleContext;
            logout_button_portfolio_css.AddProvider(cssProvider, Gtk.StyleProviderPriority.Application);
            logout_button_portfolio_css.AddClass("logout-button-portfolio");

            var portfolio_button_gtkwindow1_css = portfolio_button_portfolio.StyleContext;
            portfolio_button_gtkwindow1_css.AddProvider(cssProvider, Gtk.StyleProviderPriority.Application);
            portfolio_button_gtkwindow1_css.AddClass("logout-button-portfolio");

            // CSS Entries

            var name_entry_login_window2_css = name_entry_login_window2.StyleContext;
            name_entry_login_window2_css.AddProvider(cssProvider, Gtk.StyleProviderPriority.Application);
            name_entry_login_window2_css.AddClass("name-entry-login-window2");

            var email_entry_login_window2_css = email_entry_login_window2.StyleContext;
            email_entry_login_window2_css.AddProvider(cssProvider, 		   Gtk.StyleProviderPriority.Application);
            email_entry_login_window2_css.AddClass("email-entry-login-window2");

            var password_entry_login_window2_css = password_entry_login_window2.StyleContext;
            password_entry_login_window2_css.AddProvider(cssProvider, 		   Gtk.StyleProviderPriority.Application);
            password_entry_login_window2_css.AddClass("password-entry-login-window2");

            var password_confirm_entry_login_window2_css = password_confirm_entry_login_window2.StyleContext;
            password_confirm_entry_login_window2_css.AddProvider(cssProvider, 		   Gtk.StyleProviderPriority.Application);
            password_confirm_entry_login_window2_css.AddClass("password-confirm-entry-login-window2");

            var email_entry_login_window1_css = email_entry_login_window1.StyleContext;
            email_entry_login_window1_css.AddProvider(cssProvider, 		   Gtk.StyleProviderPriority.Application);
            email_entry_login_window1_css.AddClass("email-entry-login-window1");

            var password_entry_login_window1_css = password_entry_login_window1.StyleContext;
            password_entry_login_window1_css.AddProvider(cssProvider, 		   Gtk.StyleProviderPriority.Application);
            password_entry_login_window1_css.AddClass("password-entry-login-window1");

            var phone_number_entry_login_window3_css = phone_number_entry_login_window3.StyleContext;
            phone_number_entry_login_window3_css.AddProvider(cssProvider, 		   Gtk.StyleProviderPriority.Application);
            phone_number_entry_login_window3_css.AddClass("phone-number-entry-login-window3");

            var dob_entry_login_window3_css = dob_entry_login_window3.StyleContext;
            dob_entry_login_window3_css.AddProvider(cssProvider, 		   Gtk.StyleProviderPriority.Application);
            dob_entry_login_window3_css.AddClass("dob-entry-login-window3");

            var combo_box_entry_login_window3_css = combo_box_entry_login_window3.StyleContext;
            combo_box_entry_login_window3_css.AddProvider(cssProvider, 		   Gtk.StyleProviderPriority.Application);
            combo_box_entry_login_window3_css.AddClass("combo-box-entry-login-window3");

            var address_entry_login_window3_css = address_entry_login_window3.StyleContext;
            address_entry_login_window3_css.AddProvider(cssProvider, 		   Gtk.StyleProviderPriority.Application);
            address_entry_login_window3_css.AddClass("address-entry-login-window3");

            var search_portfolio_css = search_portfolio.StyleContext;
            search_portfolio_css.AddProvider(cssProvider, 		   Gtk.StyleProviderPriority.Application);
            search_portfolio_css.AddClass("search-portfolio");


            // CSS Card

            var card_css = card.StyleContext;
            card_css.AddProvider(cssProvider, Gtk.StyleProviderPriority.Application);
            card_css.AddClass("card");

            var main_window_css = main_window.StyleContext;
            main_window_css.AddProvider(cssProvider, Gtk.StyleProviderPriority.Application);
            main_window_css.AddClass("main-window");


            login_window1.DeleteEvent += delegate { Application.Quit(); };
            login_window2.DeleteEvent += delegate { Application.Quit(); };



            login_window1.ShowAll();


            Application.Run();
        }


// Main Window



    private void portfolio_button_gtkwindow1_clicked(object sender, EventArgs e){
        main_window.Hide();
        portfolio_window.ShowAll();


    }


    // Sorting and adding ;
    //---------Add Market Values in Main---------------
    	// index counter
    private void exchange_button_main_window_clicked(object sender, EventArgs e){

            buyFromServer();


    }

    private void AddFrameToMarketValuesMainWindow(int index)
    {




        // Create a new frame
        Frame currencyFrame = new Frame("");

        // Create the frame
        currencyFrame.Visible = true;
        currencyFrame.CanFocus = false;
        //currencyFrame.MarginTop = 10;
        //currencyFrame.MarginBottom = 10;
        currencyFrame.LabelXalign = 0;
        currencyFrame.ShadowType = ShadowType.None;

        // Create the alignment
        Alignment alignment = new Alignment(0, 0, 0, 0);
        alignment.Visible = true;
        alignment.CanFocus = false;
        //alignment.LeftPadding = 12;


        // Create the inner grid
    	Grid innerGrid = new Grid();
    	innerGrid.Visible = false;
 	   innerGrid.CanFocus = false;
 	   //innerGrid.RowSpacing = 10;
 	   //innerGrid.ColumnSpacing = 10;
 	   innerGrid.RowHomogeneous = true;
 	   innerGrid.ColumnHomogeneous = true;







        // Create the inner grid
        Grid currencyNameGrid = new Grid();
        //currencyNameGrid.MarginBottom = 9;
        //currencyNameGrid.MarginLeft = 30;
        currencyNameGrid.Visible = true;
        currencyNameGrid.CanFocus = false;
        currencyNameGrid.RowSpacing = 0;
        //currencyNameGrid.ColumnSpacing = 10;
        currencyNameGrid.RowHomogeneous = true;
        currencyNameGrid.ColumnHomogeneous = true;


        // Add child widgets to the inner grid (similar to your provided XML structure)
        // Here, you'd create and add GtkImage, GtkLabel, GtkButton, etc., to the innerGrid


        // Icon Image

        Image currencyIconImage = new Image(currencyIcons[index]);
        currencyIconImage.Visible = true;
        currencyIconImage.CanFocus = false;
        //currencyIconImage.MarginLeft = 40;
        currencyNameGrid.Attach(currencyIconImage, 0, 0, 1, 1);

        // Name Label

        Label currencyNameLabel = new Label(currencyName[index]);
        currencyNameLabel.Name = $"CurrencyName_{index}";
        currencyNameLabel.Visible = true;
        currencyNameLabel.CanFocus = false;
        currencyNameLabel.Halign = Align.Start; // Adjust horizontal alignment
	currencyNameLabel.Valign = Align.Center; // Adjust vertical alignment
        //currencyNameLabel.MarginRight = 30;


        currencyNameGrid.Attach(currencyNameLabel, 1, 0, 1, 1);


	// inner frame for currency name
        Frame currencyNameFrame= new Frame("");
        currencyNameFrame.ShadowType = ShadowType.None;
        currencyNameFrame.Add(currencyNameGrid);

        // Set fixed width for the currency Frame
        int fixedWidth = 200; // Set your desired fixed width
        currencyNameFrame.SetSizeRequest(fixedWidth, -1);

        innerGrid.Attach(currencyNameFrame, 0, 0, 1, 1);




        // Price Label

        Label currencyPriceLabel = new Label("$" + currencyPrice[index].ToString());
        currencyPriceLabel.Name = $"CurrencyPrice_{index}";
        currencyPriceLabel.Visible = true;
        currencyPriceLabel.CanFocus = false;
        //currencyPriceLabel.MarginBottom = 9;
        //currencyPriceLabel.Halign = Align.End;

        // inner frame for price
        Frame priceFrame= new Frame("");
        priceFrame.ShadowType = ShadowType.None;
        priceFrame.Add(currencyPriceLabel);

        // Set fixed width for the priceFrame
        //int fixedWidth = 150; // Set your desired fixed width
        priceFrame.SetSizeRequest(fixedWidth, -1);

        innerGrid.Attach(priceFrame, 1, 0, 1, 1);

        // Volume Label

        Label currencyVolumeLabel = new Label(currencyVolume[index].ToString());
        currencyVolumeLabel.Name = $"Volume_{index}";
        currencyVolumeLabel.Visible = true;
        currencyVolumeLabel.CanFocus = false;
        //currencyVolumeLabel.MarginBottom = 9;
        //currencyVolumeLabel.Halign = Align.End;

        // inner frame for volume
        Frame volumeFrame= new Frame("");
        volumeFrame.ShadowType = ShadowType.None;
        volumeFrame.Add(currencyVolumeLabel);

        innerGrid.Attach(volumeFrame, 2, 0, 1, 1);

        // Rank Label

        Label currencyRankLabel = new Label(currencyRank[index]);
        currencyRankLabel.Name = $"CurrencyRank_{index}";
        currencyRankLabel.Visible = true;
        currencyRankLabel.CanFocus = false;
        //currencyRankLabel.MarginBottom = 9;
        //currencyRankLabel.Halign = Align.End;

        // inner frame for rank
        Frame rankFrame= new Frame("");
        rankFrame.ShadowType = ShadowType.None;
        rankFrame.Add(currencyRankLabel);

        innerGrid.Attach(rankFrame, 3, 0, 1, 1);

        // Exchange Button
        Button exchangeButton = new Button("Exchange");
        exchangeButton.Name = $"ExchangeButton_{index}";

        //exchangeButton.MarginBottom = 9;
        //exchangeButton.MarginRight = 10;
        //exchangeButton.Halign = Align.End;

        // inner frame for echange
        Frame echangeFrame= new Frame("");
        echangeFrame.ShadowType = ShadowType.None;
        echangeFrame.Add(exchangeButton);

        innerGrid.Attach(echangeFrame, 4, 0, 1, 1);

        // Connect button click events for main_window
        exchangeButton.Clicked += exchange_button_main_window_clicked;




        // Add the inner grid to the alignment
        alignment.Add(innerGrid);

        // Add the alignment to the frame
        currencyFrame.Add(alignment);

        // Align Frame

        currencyFrame.MarginEnd = 20;



        // Add the frame to the market_values_box
        market_values_box.Add(currencyFrame);
        market_values_box.ShowAll();
    }

//----------------Sort with Labels---------------------------



// -NAME-
private void OnSortCurrencyName(object sender, ButtonPressEventArgs args)
{
    // Handle the click event
    //var label = (Label)((EventBox)sender).Child;
    //label.Text = "Clicked!";
    isAscendingOrderName = !isAscendingOrderName;
    SortFramesByCurrencyName();
}

private void SortFramesByCurrencyName()
{
    // Get all child frames in market_values_box
    var frames = market_values_box.Children
        .OfType<Frame>()
        .ToList();

    // Sort frames based on the currency name
    frames.Sort((frame1, frame2) =>
    {
        // Extract currency names from the frames
        string currencyName1 = GetCurrencyNameFromFrame(frame1);
        string currencyName2 = GetCurrencyNameFromFrame(frame2);

        // Compare currency names
        int result = currencyName1.CompareTo(currencyName2);
        return isAscendingOrderName? result: -result;
    });

    // Clear existing components in the box
    foreach (var child in market_values_box.Children.ToList())
    {
        market_values_box.Remove(child);
    }

    // Add the frames back to the box in the sorted order
    foreach (var frame in frames)
    {
        market_values_box.Add(frame);
    }

    // Show all widgets in the box
    market_values_box.ShowAll();
}

private string GetCurrencyNameFromFrame(Frame frame)
{
    // Assuming the currencyNameLabel is nested within another container within the frame
    Container container = frame.Children.FirstOrDefault() as Container;

    if (container != null)
    {
        Label currencyNameLabel = FindCurrencyNameLabelInGrid(container);
        //Console.WriteLine(currencyNameLabel);

        // Return the text of the label, or a default value if not found
        return currencyNameLabel?.Text ?? "Unknown";
    }

    // No suitable container found in the children
    return "Unknown";
}

// Helper method to find a Label widget for currency name in the children of a container
private Label FindCurrencyNameLabelInGrid(Container container)
{
    foreach (var child in container.Children)
    {
        if (child is Label label && IsCurrencyNameLabel(label))
        {
            // Found the label representing the currency name
            return label;
        }

        // If the child is a container, recursively search for a currency name label
        if (child is Container subContainer)
        {
            var currencyNameLabelInChildren = FindCurrencyNameLabelInGrid(subContainer);
            if (currencyNameLabelInChildren != null)
            {
                // Found a currency name label in a sub-container
                return currencyNameLabelInChildren;
            }
        }

        // You might need to handle other widget types based on your actual hierarchy
    }

    // No currency name label found in the children
    return null;
}

private bool IsCurrencyNameLabel(Label label)
{
    // Example: Check if the label's Name property matches a specific identifier
    return label.Name.StartsWith("CurrencyName_");
}

//------------------

// -PRICE-
private void OnSortButtonPrice(object sender, ButtonPressEventArgs args)
{
    // Handle the click event
    var label = (Label)((EventBox)sender).Child;
    //label.Text = "Clicked!";

    // Toggle the sorting order
    isAscendingOrderPrice = !isAscendingOrderPrice;

    // Handle the click event and sort frames by price
    SortFramesByPrice();
}

private void SortFramesByPrice()
{
    // Get all child frames in market_values_box
    var frames = market_values_box.Children
        .OfType<Frame>()
        .ToList();

    // Sort frames based on the price label
    frames.Sort((frame1, frame2) =>
    {
        // Extract price labels from the frames
        string priceLabel1 = GetPriceLabelFromFrame(frame1);
        string priceLabel2 = GetPriceLabelFromFrame(frame2);

        // Convert price labels to integers for comparison
        decimal price1 = ExtractPriceFromLabel(priceLabel1);
        decimal price2 = ExtractPriceFromLabel(priceLabel2);

        // Compare prices
        //return price1.CompareTo(price2);
        // Compare price values based on the sorting order
        int result = price1.CompareTo(price2);
        return isAscendingOrderPrice ? result : -result; // Reverse order if descending

    });

    // Clear existing components in the box
    foreach (var child in market_values_box.Children.ToList())
    {
        market_values_box.Remove(child);
    }

    // Add the frames back to the box in the sorted order
    foreach (var frame in frames)
    {
        market_values_box.Add(frame);
    }

    // Show all widgets in the box
    market_values_box.ShowAll();
}

private string GetPriceLabelFromFrame(Frame frame)
{
    // Assuming the currencyPriceLabel is nested within another container within the frame
    Container container = frame.Children.FirstOrDefault() as Container;

    if (container != null)
    {
        Label currencyPriceLabel = FindPriceLabelInGrid(container);
        //Console.WriteLine(currencyPriceLabel);

        // Return the text of the label, or a default value if not found
        return currencyPriceLabel?.Text ?? "$0.00"; // Default to "$0.00" if label not found
    }

    // No suitable container found in the children
    return "$0.00"; // Default to "$0.00" if container not found
}

// Helper method to find a Label widget in the children of a container
private Label FindPriceLabelInGrid(Container container)
{
    foreach (var child in container.Children)
    {
        if (child is Label label && IsPriceLabel(label))
        {
            // Found the label representing the price
            return label;
        }

        // If the child is a container, recursively search for a price label
        if (child is Container subContainer)
        {
            var priceLabelInChildren = FindPriceLabelInGrid(subContainer);
            if (priceLabelInChildren != null)
            {
                // Found a price label in a sub-container
                return priceLabelInChildren;
            }
        }

        // You might need to handle other widget types based on your actual hierarchy
    }

    // No price label found in the children
    return null;
}

private bool IsPriceLabel(Label label)
{
    // Add your criteria to identify the price label
    return label.Text.StartsWith("$"); // Example: Price label starts with "$"
}

private decimal ExtractPriceFromLabel(string priceLabel)
{
    // Assuming your price label is a string representation of a decimal
    if (decimal.TryParse(priceLabel.TrimStart('$'), out decimal price))
    {
        return price;
    }
    return 0.00m; // Default to 0.00 if parsing fails
}

//---


// -RANK- (Should start # at start)
private void OnSortButtonRank(object sender, ButtonPressEventArgs args){
	// Handle the click event
        var label = (Label)((EventBox)sender).Child;
        //label.Text = "Clicked!";

        // Toggle the sorting order
    isAscendingOrderRank = !isAscendingOrderRank;

	SortFramesByRank();
}

private void SortFramesByRank()
{
    // Get all child frames in market_values_box
var frames = market_values_box.Children
    .OfType<Frame>()  // Use OfType<T> for better readability
    .ToList();

// Sort frames based on the rank label
frames.Sort((frame1, frame2) =>
{
    // Extract rank labels from the frames
    string rankLabel1 = GetRankLabelFromFrame(frame1);
    string rankLabel2 = GetRankLabelFromFrame(frame2);

    // Convert rank labels to integers for comparison
    int rank1 = ExtractRankFromLabel(rankLabel1);
    int rank2 = ExtractRankFromLabel(rankLabel2);

    // Compare ranks
    int result = rank1.CompareTo(rank2);
    return isAscendingOrderRank ? result : -result;
});

// Clear existing components in the box
foreach (var child in market_values_box.Children.ToList())
{
    market_values_box.Remove(child);
}

// Add the frames back to the box in the sorted order
foreach (var frame in frames)
{
    market_values_box.Add(frame);
}

// Show all widgets in the box
market_values_box.ShowAll();

}

private string GetRankLabelFromFrame(Frame frame)
{
    // Assuming the currencyRankLabel is nested within another container within the frame
    Container container = frame.Children.FirstOrDefault() as Container;

    if (container != null)
    {
        Label currencyRankLabel = FindRankLabelInGrid(container);
        //Console.WriteLine(currencyRankLabel);

        // Return the text of the label, or a default value if not found
        return currencyRankLabel?.Text ?? "#0";
    }

    // No suitable container found in the children
    return "#0";
}


// Helper method to find a Label widget in the children of a container
private Label FindRankLabelInGrid(Container container)
{
    foreach (var child in container.Children)
    {
        if (child is Label label && IsRankLabel(label))
        {
            // Found the label representing the rank
            return label;
        }

        // If the child is a container, recursively search for a rank label
        if (child is Container subContainer)
        {
            var rankLabelInChildren = FindRankLabelInGrid(subContainer);
            if (rankLabelInChildren != null)
            {
                // Found a rank label in a sub-container
                return rankLabelInChildren;
            }
        }

        // You might need to handle other widget types based on your actual hierarchy
    }

    // No rank label found in the children
    return null;
}

private bool IsRankLabel(Label label)
{
    // Add your criteria to identify the rank label
    return label.Text.StartsWith("#"); // Assuming rank labels start with "#"
}

private int ExtractRankFromLabel(string rankLabel)
{
    // Assuming your rank label is in the format "#X"
    if (rankLabel.StartsWith("#") && int.TryParse(rankLabel.Substring(1), out int rank))
    {
        return rank;
    }
    return 0; // Default to 0 if parsing fails
}


// -VOLUME-

// Event handler for the sort button click
private void OnSortVolume(object sender, ButtonPressEventArgs args){
	// Handle the click event
        var label = (Label)((EventBox)sender).Child;
        //label.Text = "Clicked!";

        // Toggle the sorting order
    isAscendingOrderVolume = !isAscendingOrderVolume;

	SortFramesByVolume();
}

private void SortFramesByVolume()
{
    // Get all child frames in market_values_box
var frames = market_values_box.Children
    .OfType<Frame>()  // Use OfType<T> for better readability
    .ToList();

// Sort frames based on the rank label
frames.Sort((frame1, frame2) =>
{
    // Extract rank labels from the frames
    string volumeLabel1 = GetVolumeLabelFromFrame(frame1);
    string volumeLabel2 = GetVolumeLabelFromFrame(frame2);

    // Convert rank labels to integers for comparison
    int volume1 = ExtractVolumeFromLabel(volumeLabel1);
    //Console.WriteLine(volume1);
    int volume2 = ExtractVolumeFromLabel(volumeLabel2);
	//Console.WriteLine(volume2);

    // Compare ranks
    int result = volume1.CompareTo(volume2);
    return isAscendingOrderVolume ? result : -result;
});

// Clear existing components in the box
foreach (var child in market_values_box.Children.ToList())
{
    market_values_box.Remove(child);
}

// Add the frames back to the box in the sorted order
foreach (var frame in frames)
{
    market_values_box.Add(frame);
}

// Show all widgets in the box
market_values_box.ShowAll();

}

private string GetVolumeLabelFromFrame(Frame frame)
{
    // Assuming the currencyRankLabel is nested within another container within the frame
    Container container = frame.Children.FirstOrDefault() as Container;

    if (container != null)
    {
        Label currencyVolumeLabel = FindVolumeLabelInGrid(container);
        //Console.WriteLine(currencyRankLabel);

        // Return the text of the label, or a default value if not found
        return currencyVolumeLabel?.Text ?? "0";
    }

    // No suitable container found in the children
    return "0";
}


// Helper method to find a Label widget in the children of a container
private Label FindVolumeLabelInGrid(Container container)
{
    foreach (var child in container.Children)
    {
        if (child is Label label && IsVolumeLabel(label))
        {
            // Found the label representing the volume
            return label;
        }

        // If the child is a container, recursively search for a volume label
        if (child is Container subContainer)
        {
            var volumeLabelInChildren = FindVolumeLabelInGrid(subContainer);
            if (volumeLabelInChildren != null)
            {
                // Found a rank label in a sub-container
                return volumeLabelInChildren;
            }
        }

        // You might need to handle other widget types based on your actual hierarchy
    }

    // No rank label found in the children
    return null;
}

private bool IsVolumeLabel(Label label)
{
    // Add your criteria to identify the rank label
    return label.Name.StartsWith("Volume_"); // Assuming rank labels start with "#"
}

private int ExtractVolumeFromLabel(string volume)
{
    // Implement your logic to extract the numeric value from the volume string
    // Example: Assume the numeric value is the first part of the string
    if (int.TryParse(volume.Split(' ')[0], out int numericValue))
    {

        return numericValue;
    }

    return 0; // Default to 0 if parsing fails
}
//---
//------------------------------------------------------------------------------------------





// Window 1

        private void login_button_login_window1_clicked(object sender, EventArgs e)
        {
            loginEmail = email_entry_login_window1.Text;
            loginPassword = password_entry_login_window1.Text;


            if (IsValidEmail(loginEmail))
            {
                Console.WriteLine($"Login successful. Email: {loginEmail}, Password: {loginPassword}");
                  // Close login_window1
                login_window1.Hide();

                sendUserLoginDetails();

                main_window.ShowAll();
                requestShowServerAssets();
                requestUserPortfolio();
                sendMiningUserDetails();


            }
            else
            {
                Console.WriteLine("Invalid email address.");
                showErrorAllert("Invalid email address");
            }
        }

        private void authorization_button_login_window1_clicked(object sender, EventArgs e)
        {

            Console.WriteLine("Submit button clicked.");

            // Close login_window1
            login_window1.Hide();

            // Show login_window2
            login_window2.ShowAll();
        }

// Window 2

        private void submit_button_login_window2_clicked(object sender, EventArgs e)
        {
            // Check if the agreement CheckButton is checked
            if (agreement_login_window2.Active)
            {
                registerFullName= name_entry_login_window2.Text;
                registerEmail = email_entry_login_window2.Text;
                registerPassword = password_entry_login_window2.Text;
                registerConfirmPassword = password_confirm_entry_login_window2.Text;
    // Validate email
                if (!IsValidEmail(registerEmail))
                {
                    Console.WriteLine("Invalid email address.");
                    showErrorAllert("Invalid email address.");
                    // Optionally, you can show an error message or take other actions
                    return;
                }

                // Validate password
                if (registerPassword.Length < 7)
                {
                    Console.WriteLine("Password must be at least 7 characters long.");
                    showErrorAllert("Password less than 7 digits");
                    return;
                }
                else if (PasswordContainsDigitAndUppercase(registerPassword) == 1)
                {
                    Console.WriteLine("Password must be  contain at least one digit and one uppercase character.");
                    showErrorAllert("Password has no digits");
                    // Optionally, you can show an error message or take other actions
                    return;
                }
                else if (PasswordContainsDigitAndUppercase(registerPassword) == 2)
                {
                    Console.WriteLine("Password must be one uppercase character.");
                    showErrorAllert("Password has no Uppercase");
                    return;
                }
                else if (registerPassword != registerConfirmPassword){

                    Console.WriteLine("Passwords does not match");
                    showErrorAllert("Passwords does not match");
                    return;
                }

                // Optionally, you can close login_window2 or perform other actions
                login_window2.Hide();
                login_window3.ShowAll();
            }
            else
            {
                Console.WriteLine("Please agree to the terms before submitting.");
                showErrorAllert("Please agree to terms");

                // Optionally, you can show an error message or take other actions
            }
        }
        private bool IsValidEmail(string email)
        {
            // You can implement your email validation logic here
            return email.Contains('@');
        }

        private int PasswordContainsDigitAndUppercase(string password)
        {
            // Check if the password contains at least one digit and one uppercase character
            if (!password.Any(char.IsDigit))
            {
                return 1;
            }
            else if (!password.Any(char.IsUpper)){
                return 2;
            };
            return 0;
        }

        private void back_button_login_window2_clicked(object sender, EventArgs e)
        {
            // Add your logic for going back from login_window2 to login_window1

            // Optionally, you can close login_window2 or perform other actions
            login_window2.Hide();
            login_window1.ShowAll();
        }
        private void agreement_login_window2_toggled(object sender, EventArgs e)
        {
            // Add your logic for handling the agreement CheckButton state change
            bool isChecked = agreement_login_window2.Active;

            // Optionally, you can perform actions based on whether the CheckButton is checked or unchecked
            Console.WriteLine($"Agreement CheckButton state changed: {isChecked}");
        }

// Window 3

        private void submit_button_login_window3_clicked(object sender, EventArgs e)
        {
            registerPhoneNumber = phone_number_entry_login_window3.Text;
            registerDateOfBirth = dob_entry_login_window3.Text;
            registerAddress = address_entry_login_window3.Text;
            registerNationality = combo_box_entry_login_window3.Text;

            // Validate phone number (only digits allowed)
            if (!IsValidPhoneNumber(registerPhoneNumber))
            {
                Console.WriteLine("Invalid phone number. Please enter only digits.");
                showErrorAllert("Invalid phone number");
                // Optionally, you can show an error message or take other actions
                return;
            }

            // Validate date of birth (in the format 00/00/0000)
            if (!IsValidDateOfBirth(registerDateOfBirth))
            {
                Console.WriteLine("Invalid date of birth. Please enter the date in the format 00/00/0000.");
                showErrorAllert("Invalid Data of Birth");
                // Optionally, you can show an error message or take other actions
                return;
            }


            // Optionally, you can perform other actions, for example, submit the data or close the window
            Console.WriteLine($"Submitted data: Phone Number: {registerPhoneNumber}, Date of Birth: {registerDateOfBirth}, Address: {registerAddress}, ComboBox Value: {registerNationality}");
            sendUserRegisterDetails();

        }

        private static void OnEntryChanged(object sender, EventArgs args)
        {
            if (sender is Entry entry)
            {


                if ((entry.Text).Length == 2){


                    entry.Text += "/" ;
                    entry.Position += 1;
                }
                else if  ((entry.Text).Length == 5){
                    entry.Text += "/";
                    entry.Position += 1;

                }
                entry.Position = -1;



            }
        }

        private bool IsValidPhoneNumber(string phoneNumber)
        {
            // Check if the phone number contains only digits
            return phoneNumber.All(char.IsDigit);
        }

        private bool IsValidDateOfBirth(string dob)
        {
            // Validate date of birth format (00/00/0000)
            if (DateTime.TryParseExact(dob, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out _))
            {
                return true;
            }
            return false;
        }

        private void back_button_login_window3_clicked(object sender, EventArgs e)
        {
            GoBackToWindow2();
        }

        private void GoBackToWindow2()
        {
            // Optionally, you can perform other actions before switching to login_window2
            // For example, you might want to clear the entries in login_window3.

            // Close login_window3
            login_window3.Hide();

            // Show login_window2
            login_window2.ShowAll();
        }


// Transactions Window

        private void AddFrameToTransactionWindow(int index)
        {




            // Create a new frame
            Frame currencyFrame = new Frame("");

            // Create the frame
            currencyFrame.Visible = true;
            currencyFrame.CanFocus = false;
            //currencyFrame.MarginTop = 10;
            //currencyFrame.MarginBottom = 10;
            currencyFrame.LabelXalign = 0;
            currencyFrame.ShadowType = ShadowType.None;

            // Create the alignment
            Alignment alignment = new Alignment(0, 0, 0, 0);
            alignment.Visible = true;
            alignment.CanFocus = false;
            //alignment.LeftPadding = 12;


            // Create the inner grid
            Grid innerGrid = new Grid();
            innerGrid.Visible = false;
            innerGrid.CanFocus = false;
            //innerGrid.RowSpacing = 10;
            //innerGrid.ColumnSpacing = 10;
            innerGrid.RowHomogeneous = true;
            innerGrid.ColumnHomogeneous = true;







            // Create the inner grid
            Grid currencyNameGrid = new Grid();
            //currencyNameGrid.MarginBottom = 9;
            //currencyNameGrid.MarginLeft = 30;
            currencyNameGrid.Visible = true;
            currencyNameGrid.CanFocus = false;
            currencyNameGrid.RowSpacing = 0;
            //currencyNameGrid.ColumnSpacing = 10;
            currencyNameGrid.RowHomogeneous = true;
            currencyNameGrid.ColumnHomogeneous = true;


            // Add child widgets to the inner grid (similar to your provided XML structure)
            // Here, you'd create and add GtkImage, GtkLabel, GtkButton, etc., to the innerGrid


            // Icon Image

            Image currencyIconImage = new Image($"GUI/Glade/images/icons/{userTransactionsList[index].CryptocurrencyName}.png");
            currencyIconImage.Visible = true;
            currencyIconImage.CanFocus = false;
            //currencyIconImage.MarginLeft = 40;
            currencyNameGrid.Attach(currencyIconImage, 0, 0, 1, 1);

            // Name Label

            Label currencyNameLabel = new Label(userTransactionsList[index].CryptocurrencyName);
            currencyNameLabel.Name = $"CurrencyName_{index}";
            currencyNameLabel.Visible = true;
            currencyNameLabel.CanFocus = false;
            currencyNameLabel.Halign = Align.Start; // Adjust horizontal alignment
            currencyNameLabel.Valign = Align.Center; // Adjust vertical alignment
            //currencyNameLabel.MarginRight = 30;


            currencyNameGrid.Attach(currencyNameLabel, 1, 0, 1, 1);


            // inner frame for currency name

            Frame currencyNameFrame= new Frame("");
            currencyNameFrame.ShadowType = ShadowType.None;
            currencyNameFrame.Add(currencyNameGrid);

            // Set fixed width for the currency Frame
            int fixedWidth = 200; // Set your desired fixed width
            currencyNameFrame.SetSizeRequest(fixedWidth, -1);

            innerGrid.Attach(currencyNameFrame, 0, 0, 1, 1);




            // Date Label

            DateTime transactionDateTime = userTransactionsList[index].DateTime;

            Label currencyDateLabel = new Label($"{transactionDateTime}");
            currencyDateLabel.Name = $"CurrencyDate_{index}";
            currencyDateLabel.Visible = true;
            currencyDateLabel.CanFocus = false;
            //currencyDateLabel.MarginBottom = 9;
            //currencyDateLabel.Halign = Align.End;

            // inner frame for Date
            Frame DateFrame= new Frame("");
            DateFrame.ShadowType = ShadowType.None;
            DateFrame.Add(currencyDateLabel);

            // Set fixed width for the DateFrame
            //int fixedWidth = 150; // Set your desired fixed width
            DateFrame.SetSizeRequest(fixedWidth, -1);

            innerGrid.Attach(DateFrame, 1, 0, 1, 1);

            // Volume Label

            Label currencyVolumeLabel = new Label(userTransactionsList[index].CryptoValue);
            currencyVolumeLabel.Name = $"Volume_{index}";
            currencyVolumeLabel.Visible = true;
            currencyVolumeLabel.CanFocus = false;
            //currencyVolumeLabel.MarginBottom = 9;
            //currencyVolumeLabel.Halign = Align.End;

            // inner frame for volume
            Frame volumeFrame= new Frame("");
            volumeFrame.ShadowType = ShadowType.None;
            volumeFrame.Add(currencyVolumeLabel);

            innerGrid.Attach(volumeFrame, 2, 0, 1, 1);

            // Fee Label

            Label currencyFeeLabel = new Label($"{userTransactionsList[index].TransactionFee}");
            currencyFeeLabel.Name = $"CurrencyFee_{index}";
            currencyFeeLabel.Visible = true;
            currencyFeeLabel.CanFocus = false;
            //currencyFeeLabel.MarginBottom = 9;
            //currencyFeeLabel.Halign = Align.End;

            // inner frame for Fee
            Frame FeeFrame= new Frame("");
            FeeFrame.ShadowType = ShadowType.None;
            FeeFrame.Add(currencyFeeLabel);

            innerGrid.Attach(FeeFrame, 3, 0, 1, 1);



            // Add the inner grid to the alignment
            alignment.Add(innerGrid);

            // Add the alignment to the frame
            currencyFrame.Add(alignment);

            // Align Frame

            currencyFrame.MarginEnd = 20;



            // Add the frame to the market_values_box
            transactions_box.Add(currencyFrame);
            transactions_box.ShowAll();
        }
// Navigation bar functions

        private void transactions_button_main_window_clicked(object sender, EventArgs args){
            main_window.Hide();

            transactions_window.ShowAll();
            requestTransactionList();
        }

// Main Functions


         static void Main(){
             new CCTPSApp();
        }

    }
}
