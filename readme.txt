welcome to tc16 - clarity hearts.

your task is to implement a hearts player capable of stomping the 
competition. this document contains rules and advice towards that end.

dev guide
=========

note that this sdk includes all source code for the project. this is 
intended to make development smoother for you. you are encouraged to report 
bugs as you come across them.

the code is currently undocumented. this will change in the near future.

to create a player, you must inherit from Hearts.Models.PlayerBase. doing so 
requires implementing several methods:

	public abstract string Name { get; }
	public abstract IEnumerable<Card> GetCardsToPass(PlayerView toPlayer);
	public abstract Card GetLead(DealView dealInProgress);
	public abstract Card GetPlay
        (DealView dealInProgress, TrickView trickInProgress);

optionally, you may also override any of these methods:

	public virtual void NotifyPassedCards
        (IEnumerable<Card> cards, PlayerView fromPlayer);
	public virtual void NotifyTrickResults(TrickView completedTrick);
	public virtual void NotifyDealResults(DealView completedDeal);

when implementing GetLead and GetPlay, you will find two helpful extension 
methods in Hearts.Models.Extensions:

	public static IEnumerable<Card> GetLeadableCards
        (this PlayerBase player, DealView dealInProgress);
	public static IEnumerable<Card> GetPlayableCards
        (this PlayerBase player, TrickView trickInProgress);

these methods will filter your player's hand to only those cards legal to 
play during any given trick. since the rules around play and lead legality 
are slightly complex, i strongly suggest you use these methods.

in the event that you do attempt to make an invalid play or otherwise throw 
an exception or timeout, a choice will be made for you:

	for GetCardsToPass, the first three cards from your sorted hand
	for GetLead, the first card from GetLeadableCards
	for GetPlay, the first card from GetPlayableCards

(n.b. timeouts are not yet implemented, but you will be limited to 1s for 
each of the aforementioned methods.)

regarding Card: Card is a value type with all the appropriate equality
methods overridden correctly. this means that, when comparing two cards,
only their values are taken into account. so you can say things like:

	if (card == new Card(Suit.Spades, Rank.Queen))
		...

and have it work the way you expect.

Hearts.Models.Extensions contains many useful extension method for working 
with individual cards and collections of cards. feel free to explore and 
offer suggestions.

dev tools
=========

to test your player, you may use the included console app. the app prints 
helpful debug info as your game proceeds. note that there are still a few 
deficiencies. (e.g. the printed results of card passing are less helpful than 
i would like.) these will be improved in the next few days.

also, please forgive some slight delays after some commands. these are due 
to some implementation details i will hopefully be able to clean up shortly.

the console recognizes the following commands:

	q: quit
	r: restart game
	t: run test hand
	spacebar: pause/resume
	enter: when paused, step

more commands may be added as i think of them.

some info regarding "test hands": these are configurable via app.config. i 
think if you take a look you'll find it very straightforward.

a ui will be included with future versions of this sdk.

note that you can configure the assembly used in app.config. this should also 
be very straightforward. currently, only one player dll is supported. i.e. 
four instances of your player will be created to play against themselves. 
support for more than one assembly will be included in the future.

now that players are loaded via config, you shouldn't have any need to edit 
the console app. but feel free to poke around, if you like.

hearts ui
=========

i'm just going to talk about how to set up your avatar correctly. (if you're
curious about anything else, feel free to contact me directly.) there are 
just two things you need to do.

first, add whatever image to your project and set its build action to
embedded resource under the properties dialog.

second, you need to override the Avatar property on your player and return
the appropriate resource name. the resource name is simple to derive if you
think of your resource as a class in your project. it is simply your
project's default namespace followed by the directory path to your image
followed by your image name. all of this should be '.' delimited. for example,
if my project has a default namespace of My.Sample.Project and my image is in
My/Image/Directory with a filename of myimage.png, then my resource name is:

My.Sample.Project.My.Image.Directory.myimage.png

of course, the simplest thing to do is just to put the image in the top level.
if you're not sure about your default namespace, you can find it in your project
properties.

to test this in the ui, compile your player and copy the dll into bin/debug of
the ui project. start up the ui and find your player's name in the players menu.
after selecting it, start a new game in the game menu. a game with your player
and three drones will start. after taking a moment to bask in the glorious 
animations, catch your breath and confirm that your avatar has appeared in your 
player's info pane.

if any of this is unclear, or if you can't get your avatar to work, ping me.

also note that you can change your avatar mid-game simply by supplying a 
different resource name.

tournament info
===============

the tournament is projected for a mid-may date. more details will follow as 
this gets nailed down.

tournament format is likewise tbd.

there will be some kind of prizes. in past tech challenges, these have taken 
the form of (amazon?) gift cards.

you are free to team up, but only one entry per team is permitted.

collusion is forbidden. this includes any method of communication between 
entries.

reflection is forbidden, as is unsafe code.
