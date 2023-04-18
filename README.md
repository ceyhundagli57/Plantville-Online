# Plantville-Online
Online version of Plantville 


When you are in a textbox, you should be able to press Enter and the submit button should automatically execute. This should happen in most textboxes.
Sign-in page:

Players open the app to a sign-in page. Players can sign in as whatever name they want. This will be their username whenever they post a chat message or submit a trade.
Inventory

Update page so instead of listing every plant you have, list them by unique plants and the quantity owned. If there are 0 plants (e.g. 0 strawberries), it should be removed from the list. 
Update save to file feature to reflect this update.
 Chatroom
Displays the latest chat messages.
Task: Make a GET request to the server. More info on API below.
Players can post new chat messages. 
Task: Make a POST request to server. More info on API below.
This chatroom only updates when you first enter the chatroom or after submitting a new message. This won't auto-update (to make it simpler for you). This means you have to click the "Chat" button on the left hand side to get new updates.
Propose trade
Propose a trade that appears on the Marketplace. Once completed, it should clear the fields and show confirmation message.
Task: Make a POST request to server.
Plant ComboBox is populated from seed_list variable (the list of seeds we can buy).

Side note: Good developers see this as an obvious implementation. We want our data to expand and contract from one source (e.g. seed_list). 
Validation: 
Verify player has enough money to propose trade. 
Trade Marketplace
On first load, displays latest trades.
Task: Make GET request to server
Each trade has a unique ID.  That ID is called "pk", which stands for Primary Key. This is the ID of the trade we will be using when we ask for it.
Player selects a trade from ListBox, then accepts it. 
Accepting trades: There are two parties to a trade -- the author and the receiver. 

Author proposed the trade.
Receiver accepted the trade conditions.
When author proposes a trade:
It appears immediately on the trade marketplace.
This part is weird. Ask questions if confusing.

Inventory and money exchange does not occur immediately. Author must wait until a person on the marketplace accepts the trade. The first time an author returns to Marketplace after their trade has been accepted, the inventory and money exchange will adjust.
When receiver accepts a trade:
Send message box showing trade has been accepted.
Adjust inventory and money exchange immediately.
Close the trade.
Task: Make POST request to server.
Validation:
Verify player selected a trade before clicking accept button.
Verify player did not accept a trade they proposed. Can only accept trades from other players.
Verify player did not accept a closed trade. Can only accept open trades.
Verify player has enough resources to accept trade. If trade requests 3 strawberries, player must have at least 3 strawberries in their inventory.

API
Below is an explanation of the URLs, the http methods (e.g. POST or GET), and the variables we need to pass to the server.

Chat messages - http://plantville.herokuapp.com/Links to an external site.
GET retrieves last 100 chat messages in JSON.
POST adds a new chat message.

Returns last 100 chat messages after adding new chat message.
username: the name of player sending chat message
message: chat message
Trades - http://plantville.herokuapp.com/tradesLinks to an external site. 
GET retrieves last 100 trade proposals
POST adds a new trade proposal.

Returns the ID of the new trade. This is useful for tracking which trades are pending for authors waiting for their trade to be accepted.
author: name of player sending trade proposal
plant: name of plant player wants to buy
quantity: amount of plan they want (e.g. 3 strawberries)
price: price player willing to pay
Accept trade - https://plantville.herokuapp.com/accept_tradeLinks to an external site.
POST accepts the trade
trade id: The ID given when adding a new trade proposal
accepted_by: the name of player accepting trade proposal
