using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text.RegularExpressions;

#if !COMPILER_UDONSHARP && UNITY_EDITOR
using UdonSharpEditor;
#endif

public class DrunkCardsQuestions : UdonSharpBehaviour {
  public int myVersion = 1;
  [UdonSynced] public int currentVersion = 0;

  public Text textBox;
  public Text usernameText;
  public Toggle masterToggle;
  public AudioSource clickSound;
  [UdonSynced] public int currentText = -1;
  [UdonSynced] public string currentUsername = "";
  public int previousText = -1;
  [UdonSynced] public bool masterLocked = false;
  [UdonSynced] public string template0 = "";
  [UdonSynced] public string template1 = "";
  [UdonSynced] public string template2 = "";

  private float[] weights = {0.15f, 0.25f, 0.25f, 0.2f, 0.1f, 0.05f};

  public Vector3 center = Vector3.zero;
  public Vector3 size = Vector3.one;
  private Bounds roomBounds;

  private VRCPlayerApi[] playerBuf = new VRCPlayerApi[64];
  private VRCPlayerApi[] playerBuf2 = new VRCPlayerApi[64];

  string[] questions = new[] {
    // Challenges:
    "The others choose someone on your Discord friends list and create a message for you to message them with. You can choose to send the message or take {2}",
    "Take {2} and Impersonate someone in the room",
    "Get down on one knee and propose to the person on your left or take {2}",
    "Take a drink and Sing everything you say for {1}",
    "Take someone with you and go avatar shopping. When you come back everyone decides if they like your new avatar. If they don't like it take 2 shots",
    "Talk in an accent for the next {1} and take a drink every time you forget",
    "Attempt to dance for 30 seconds or take {2}",
    "Let the group give you a new avatar to use for {1} or take {2}",
    "Take a shot and do the worm, you don't have a say in this",
    "Take a shot and Curse like sailor for 20 seconds straight",
    "Put 2 ice cubes down your pants or take 2 shots",
    "Let the group pose you in an embarrassing position and take a picture or take 2 shots",
    "Take 2 shots and depict a human life through interpretive dance",
    "Make everyone in the lobby laugh if you give up take a shot for every person you didn't make laugh",
    "Take a shot and Sell a piece of trash to someone in the group. Use your best salesmanship.",
    "Imitate popular YouTuber if someone guesses it they can gift a shot, if no one can guess it, you take a shot",
    "Pick something in your room, spell it without opening your mouth, If someone gets it they can gift 2 shots, if no one guesses it you take two shots",
    "Take a shot and Jump up and down as high as you can go for a full minute",
    "Take a drink switch to an avatar of the opposite gender than you have now",
    "Take a buddy shot with {0} and switch avatars",
    "For the next round take only water shots",
    // Truths:
    "Answer the truth or take a shot: What's your dream first date?",
    "Answer the truth or take a shot: What is something you look for in a partner?",
    "Answer the truth or take a shot: Have you ever gotten in a physical altercation with someone?",
    "Answer the truth or take a shot: What's your longest relationship?",
    "Answer the truth or take a shot: What's the biggest lie you've ever told?",
    "Answer the truth or take a shot: Who is your favorite family member?",
    "Answer the truth or take a shot: Do you have a bucket list? If so, what is one thing on that list?",
    "Answer the truth or take a shot: What is the one thing you dislike about yourself?",
    "Answer the truth or take a shot: What is the one thing you really like about yourself?",
    "Answer the truth or take a shot: If you could hire someone to do one thing for you, what would it be?",
    "Answer the truth or take a shot: What is your biggest pet peeve?",
    "Answer the truth or take a shot: Top or bottom?",
    "Answer the truth or take a shot: Would you hook up with your high school crush today?",
    "Answer the truth or take a shot: Have you ever joined a hook-up app or any app associated with risky behavior?",
    "Answer the truth or take a shot: What's the most outrageous thing you can remember doing while \"under the influence?\"",
    "Answer the truth or take a shot: Are you more dominant or submissive?",
    "Answer the truth or take a shot: Describe your, \"I'm getting laid tonight\" outfit?",
    "Answer the truth or take a shot: How many people have you kissed?",
    "Answer the truth or take a shot: Who is the most annoying person you know?",
    "Answer the truth or take a shot: Have you ever had a dream about someone in the game?",
    "Answer the truth or take a shot: Who are the 3 people you trust most?",
    "Answer the truth or take a shot: Have you ever had sexual fantasies about someone in this room?",
    "Answer the truth or take a shot: Have you ever stolen something?",
    "Answer the truth or take a shot: Who is someone you hate?",
    "Answer the truth or take a shot: If you had to sleep with someone in this room, who would it be?",
    "Answer the truth or take a shot: What's the biggest secret you're keeping from everyone in this room?",
    "Answer the truth or take a shot: What has been your most embarrassing moment so far?",
    "Answer the truth or take a shot: What's the last thing you Googled?",
    // Story:
    "Tell us about the last dream you can remember or take {2}",
    "Take a drink and tell us some words of wisdom",
    "Take a drink and tell us about someone you'll always remember?",
    "Take a buddy shot and have the other person tell you their favorite memory of you",
    "Take a buddy shot and ask the other person If this was to be our very last conversation, is there anything you'd want to say to me",
    "Take a buddy shot and ask them: Is there anything that you've never told me but want to tell me now?",
    "Take a shot and have someone else describe you",
    // Would You rather:
    "Take a drink and If you had to choose between going naked or having your thoughts appear in thought bubbles above your head for everyone to read, which would you choose",
    "Take a drink and Would you rather have a golden voice or a silver tongue?",
    "Take a drink and Would you rather be in jail for a year or lose a year off your life?",
    "Take a drink and Would you rather always be 10 minutes late or always be 20 minutes early?",
    "Take a drink and Would you rather have one real get out of jail free card or a key that opens any door?",
    "Take a drink and Would you rather know the history of every object you touched or be able to talk to animals?",
    "Take a drink and Would you rather have all traffic lights you approach be green or never have to stand in line again?",
    "Take a drink and Would you rather be able to see 10 minutes into your own future or 10 minutes into the future of anyone but yourself?",
    "Take a drink and Would you rather go back to age 5 with everything you know now or now know everything your future self will learn?",
    "Take a drink and Would you rather be able to control animals (but not humans) with your mind or control electronics with your mind?",
    "Take a drink and Would you rather be forced to dance every time you heard music or be forced to sing along to any song you heard?",
    "Take a drink and Would you rather be able to dodge anything no matter how fast it's moving or be able to ask any three questions and have them answered accurately?",
    "Take a drink and Would you rather 5% of the population have telepathy, or 5% of the population have telekinesis? You are not part of the 5% that has telepathy or telekinesis.",
    "Take a drink and Would you rather suddenly be elected a senator or suddenly become a CEO of a major company? (You won't have any more knowledge about how to do either job than you do right now.)",
    "Take a drink and Would you rather live in virtual reality where you are all powerful or live in the real world and be able to go anywhere but not be able to interact with anyone or anything?",
    "Take a drink and Would you rather know how above or below average you are at everything or know how above or below average people are at one skill/talent just by looking at them?",
    "Take a drink and Would you rather your only mode of transportation be a donkey or a giraffe?",
    "Take a drink and Would you rather have all dogs try to attack you when they see you or all birds try to attack you when they see you?",
    "Take a drink and Would you rather randomly time travel +/- 20 years every time you fart or teleport to a different place on earth (on land, not water) every time you sneeze?",
    "Take a drink and Would you rather become twice as strong when both of your fingers are stuck in your ears or crawl twice as fast as you can run?",
    "Take a drink and Would you rather have everything you draw become real but be permanently terrible at drawing or be able to fly but only as fast as you can walk?",
    "Take a drink and Would you rather thirty butterflies instantly appear from nowhere every time you sneeze or one very angry squirrel appears from nowhere every time you cough?",
    "Take a drink and Would you rather be able to control fire or water",
    "Take a drink and Would you rather have a bottomless box of LEGOs or a bottomless gas tank?",
    "Take a drink and Would you rather be lost in a bad part of town or lost in the forest?",
    "Take a drink and Would you rather be completely invisible for one day or be able to fly for one day?",
    "Take a drink and Would you rather have amazingly fast typing/texting speed or be able to read ridiculously fast?",
    "Take a drink and Would you rather all electrical devices mysteriously stop working (possibly forever) or the governments of the world are only run by people going through puberty?",
    // Drinks:
    "Take a drink. That's all.",
    "Choose someone to take a drink",
    "Take a shot with someone",
    "Everyone Takes a shot",
    "A Drink for everyone!",
    "Refill your drink",
    "Finish your drink or take 3 shots if you're a pansy",
    "Next time you have to drink, pass it on to someone else",
    "Next time someone else has to drink, you drink it instead",
    "Guess what! Take a shot!",
    "Everyone has to take 2 shots now, Good job!",
    "Mutes Drink! If there are no mutes well take 3 shots, someone has to suffer",
    "Fullbody users Drink! If there are none then go get a better job",
    "E-boys Drink, yes I'm looking at you",
    "E-girls drink or guys cause we know there are no girls on the internet",
    "VR users drink!",
    "Desktop users Drink!",
    // Drunk Pirate
    "If yuo cna raed tihs tehn dinrk",
    "Drink if you can read this",
    "Anyone on their phone right now must drink",
    "Think of a number from 1-10, the other players must go clockwise guessing the number, whoever gets it correct can choose someone to drink",
    "Everyone stand up, the last person standing must drink",
    "Say the alphabet backwards or drink",
    "90s kids drink",
    "Drum a beat on the table, if the player on your right cannot repeat it they must drink",
    "Play this round with your eyes closed",
    "Whoever has travelled to the most countries must drink",
    "You have to drink double for one round",
    "Touch your toes without bending your legs or take a drink",
    "Drink if it's past midnight",
    "Hug any player",
    "If you would have sex with another player take a drink",
    "Have a thumb war with the player closest",
    "Anyone wearing lipstick must drink, and kiss someone on the cheek",
    "Tell the player on your left that you love them, passionately",
    "Everyone say their favorite color, any players that say the same color must drink",
    "Anyone who has eaten during this game must drink",
    "The next person to make eye contact with you must drink",
    "Drink something you haven't drunk today",
    "Anyone born in summer drink",
    "Cat owners drink",
    "High five the player to your right and then both take a drink",
    "Females drink",
    "Everyone who has taken a picture tonight must take a drink",
    "Blondes drink",
    "Drink",
    "Dog owners drink",
    "Have an arm wrestle with the player on your right, the loser must down their drink",
    "Everyone pass their drink to their left and let them take a drink",
    "The player with the weakest drink must take a drink",
    "Drink if you've ever been in a fight",
    "For one round if anyone answers a question asked by you they must drink",
    "Tell everyone your favorite book and then drink",
    "Take off a piece of clothing or drink",
    "Push the player on your left",
    "Everyone stand on one leg, the first to fall must drink",
    "Everyone vote who the best looking player is, they must down their drink",
    "You choose which player has the nicest smile, they must drink",
    "If your best friend is playing then you must both drink",
    "Everyone must try to call this player, the first to succeed can choose someone to drink",
    "Every player must read their last text/Discord message out loud or drink",
    "The most formally dressed person must drink",
    "Do 10 push ups, if you can't then drink",
    "Starting with you going clockwise from 1, you can either say one, two or three numbers. For example: one, two three, four. The person who says 21 must down their drink",
    "You and the player on your right say your favorite movies, the rest of the players vote which movie is worst, the player with the most votes must drink",
    "Anyone who has had sex with more than 10 people must drink",
    "Compliment the person on your left",
    "Everyone choose which side they prefer, tea or coffee, the smallest side must all drink",
    "Pick a player and make out with them",
    "Everyone must touch a wall, the last person to do so must drink",
    "Anyone who can play an instrument drink",
    "Anyone who has cried in the last month must drink in sorrow",
    "Tell everyone your favorite hobby then drink",
    "Stoners drink",
    "The player on your right must tell you a joke, you must decide if it's good enough, if it's not they must drink",
    "You cannot swear for {1}. If you swear then you must drink",
    "Blue eyed players drink",
    "Command each player to do anything you want, if they cannot they must drink",
    "Drink if you can't remember your last card",
    "Drink for how many countries you've been to",
    "The player with the straightest teeth must drink",
    "All players must touch you, the last to do so must drink",
    "Finish your drink in under 10 seconds, if you fail then finish that drink and one more",
    "Any players that met today must drink",
    "The player left of you must drink",
    "Starting with you go round clockwise naming Dog Breeds, the first to repeat one or not be able to think of one must drink",
    "Choose someone to down their drink",
    "Everyone vote who the most sober player is, they must down their drink",
    "Starting with you go round clockwise naming Countries in Europe, the first to repeat one or not be able to think of one must drink",
    "Anyone who owns an Apple product must drink",
    "Wine drinkers drink",
    "Do whatever the player on your left wants, if you can't then drink",
    "Tell everyone two truths and one lie Every player must guess which one is a lie. Every player that gets it wrong must drink",
    "Every player must make an animal noise and then take a drink",
    "The player showing the most skin must drink",
    "Anyone who has been sick while drinking must drink",
    "Turn away from the other players and then one player has to tap your shoulder, if you guess who it was correctly everyone else drinks but if you are wrong you must drink",
    "Anyone with a one syllable name must drink",
    "Anyone who isn't straight must drink",
    "You may shout \"chug\" at any point and everyone has to finish their drinks. Can only be used once",
    "Play rock paper scissors with the player on your left, loser drinks",
    "Kiss the closest player to you on the forehead",
    "The player with the biggest hands must take a drink",
    "Drink if you have a piercing",
    "Pick a player to drink with you",
    "Slap the bum of the player on your right",
    "The last person to raise their drink must take a drink",
    "Say your guilty pleasure",
    "Choose which two players look the most alike, they must both drink",
    "Any player who has spilt a drink tonight must drink",
    "Males drink",
    "If this is your first time playing drunk cards you must drink",
    "The male with the longest hair must drink",
    "Starting with you go round clockwise naming music genres, the first to repeat one or not be able to think of one must drink",
    "Everyone must do a silly walk and then drink",
    "Call someone and tell them you love them",
    "Anyone who claims they are not drunk must drink",
    "You choose which player has the nicest eyes, they must drink",
    "If you can name the song playing right now choose someone to drink, if there is no music then you must drink",
    "Just drink",
    "The next player who wants to leave the room must finish their drink",
    "Kiss the player on your right or both take a drink",
    "Drink if you have been single for 6 months or more",
    "Drink if English isn't your native language",
    "Use only your left hand for one round",
    "Player to your right must drink",
    "Kiss Every Player",
    "All single people must drink",
    "If there are more females and males then females must drink, if there are more males than females then males must drink",
    "Down your drink",
    "Go round clockwise pointing at every player and saying their real name, if you can't do it then you must drink",
    "Everyone choose which side they prefer, pizza or burgers, the smallest side must all drink",
    "Starting with you go round clockwise naming types of natural disaster, the first to repeat one or not be able to think of one must drink",
    "Everyone must touch the floor, the last to do so must drink",
    "You must keep one hand on your crotch for a round",
    "Tell the player on your right a riddle, if you can't think of one or they guess correctly then you must drink, if they can't figure it out they must drink",
    "Take off a piece of clothing or drink",
    "Pick three players to drink",
    "Hairiest player drinks",
    "The player opposite you must drink",
    "Down your drink and then take a shot",
    "Kiss as many players as you want, drink for the number of players you don't kiss",
    "Single people drink",
    "You must hold hands with the player on your left for one round",
    "Fill your drink up",
    "Everyone ignore this player for one round",
    "Ask the player on your right a general knowledge question, if they are correct then you drink, but if they are wrong they must drink",
    "Everyone raise their drinks and say cheers",
    "Smokers drink",
    "Starting with you go round clockwise naming US Presidents, the first to repeat one or not be able to think of one must drink",
    "Drink for every sibling you have",
    "Tell everyone where and how you met the player on your left",
    "All players must end every sentence with \"not\" for one round. Every time someone forgets they must drink",
    "The player with the largest ears must drink",
    "Everyone must compliment you or drink",
    "Vegans drink",
    "Everyone wearing glasses must drink",
    "You can't tell the truth for a whole round. Drink if anyone catches you telling the truth",
    "Tell a joke, if no one laughs then you must drink",
    "Drink for every letter in your name",
    "Redheads drink",
    "Try walk in a straight line, if you fail then you must drink",
    "The players on your right and left must kiss each other or drink",
    "Starting with you, go round clockwise saying situations like \"who is the most likely to pass out tonight?\", count down from three and everyone must point at the player they think is most likely. They must drink",
    "Spin a bottle and whoever it lands on must drink",
    "Players with tattoos drink",
    "You must only whisper until your next turn",
    "Vegetarians drink",
    "You must smell every player, the best smelling player must drink",
    "Swap avatar with the closest player of the opposite sex",
    "Choose a word and then everyone must go round clockwise saying words that rhyme with that word, the first one to fail must drink",
    "All players with the same drink as you must drink",
    "The tallest player must stand up for one round",
    "Insult the player in front of you",
    "Oldest player drinks",
    "Everyone drink",
    "Everyone vote who the smartest player is, they must down their drink",
    "The shortest player must sit on the floor for one round",
    "The player on your right must find something and bring it to you as a gift",
    "Brown eyed players drink",
    "Starting with you go round clockwise naming capital cities, the first to repeat one or not be able to think of one must drink",
    "You can create a rule that players must follow or drink, for example: \"always be holding your drink\" or \"don't look at your phone\"",
    "If you've ever kissed one of the other players then you must drink",
    "Everyone make a strange face and then drink",
    "Youngest player drinks",
    "Anyone drinking beer must drink",
    "Everyone must dance for you, you can choose who does the worst dance and they must drink",
    "Hum a song, the first player to guess the song can choose someone to drink",
    "Try touch the ceiling without jumping, if you can't then you must drink",
    "Stand on one leg and drink",
    "Choose a theme. Everyone must go round and name something from that theme until someone can't, the loser must drink. For example: You choose colors and the group must go round naming colors",
    "Play Truth or Dare",
    "Go round clockwise naming car brands, the first person not able to think of a new one must drink",
    "Tilt your head all the way back and take a drink",
    "If you enjoy the taste of your drink, then take a drink",
    "Starting with you go round clockwise naming Countries in Asia, the first to repeat one or not be able to think of one must drink",
    "Kiss either the player on your left or right, you choose...",
    "Green eyed players drink",
    "Unemployed Drink",
    "Stay silent for one round or drink",
    "Any players wearing makeup must drink",
    "On the count of three, all players must show a number on their hand from one to five, any players with the same number must drink",
    "All players start drinking and can't stop until you stop",
    "The girl with the shortest hair must drink",
    "Have a staring competition with {0}, the loser must drink",
    "Anyone drinking vodka must drink",
    "Fill up your cup and then down it",
    "Everyone look at the ground and on the count of three look up and look at another player's eyes. Any players making eye contact with each other must drink",
    "The most pale player must drink",
    "You must drink once, and then the player to your left must drink twice, and then the player to their left three times and so on until it comes back to you",
    "Anyone born in winter drink",
    "The player with the most cash on them must drink",
    "Tell everyone your biggest fear and then drink",
    "Everyone put their hands in the air, the last to do so must drink",
    "Quote a movie, if the player on your left cannot guess the movie they must drink",
    "Say a statement about yourself, it can be a truth or lie, the other players must collectively decide if it's a truth or a lie. If they are incorrect they must drink, but if they are correct you must drink",
    "Take a drink without using your hands",
    "Dance until your next round or down your drink",
    "You must say the date of birth of the player ahead of you, if correct they drink, if incorrect you drink",
    "Starting with you, go round clockwise complimenting each other",
    "Draw something on a player of your choice",
    "Choose a player, everyone but you and that player must drink",
    "Say five words starting with \"T\" in five seconds or drink",
    "For one round if anyone checks their phone they must drink",
    "Pick two players, they have to kiss each other or both take a drink",
    "Most buff player must drink",
    "Take a drink",
    "Starting with you go round clockwise naming movie genres, the first to repeat one or not be able to think of one must drink",
    "Whoever left the room last must take a drink",
    "Anyone religious drink",
    "Race the player across from you to finish a drink, the loser must take another drink",
    "Anyone with a job must drink",
    "Flex and drink",
    "Anyone with dyed hair must drink",
    "Ask the player to the left of you a science question, if they are correct you drink, if not they drink",
    "Play would you rather",
    "Starting with you go round clockwise naming Fruit, the first to repeat one or not be able to think of one must drink",
    "Play one round of never have I ever",
    "Everyone point at a player, the player with the most fingers pointed at them must drink",
    "If you are drinking someone else's drink, or someone paid for your drink: Thank them and take a drink",
    "Everyone must drink double for one round",
    "The most kinky player must drink",
    "Everyone vote who has the best haircut, the player with the most votes must take a drink",
    "Starting with you go round clockwise naming different currencies, the first to repeat one or not be able to think of one must drink",
    "Atheists drink",
    "Do a dance or take a drink",
    "Everyone vote if they prefer dogs or cats, the side with the least votes must drink",
    "Everyone looking at this card must drink",
    "Lie on your back and drink",
    "Starting with you go round clockwise naming US States, the first to repeat one or not be able to think of one must drink",
    "For one round: anyone who touches the floor must drink",
    "Stay completely still like a statue for one round",
    "Everyone choose which side they prefer, Red or Blue, the smallest side must all drink",
    "Drink twice",
    "Starting with you go round clockwise naming Beatles Songs, the first to repeat one or not be able to think of one must drink",
    "Pick 3 adjectives to describe the person on your left and then both drink",
    "The player you have known the longest must drink",
    "Drink three times",
    "You can create a rule that players must follow or drink, for example: \"no players touch the floor\" or \"no one say the word what\"",
    "The person with the biggest shoe size must drink",
    "The player with the biggest nose must drink",
    "Bearded players drink",
    "You can create a rule that players must follow or drink, for example: \"no one can say any name\" or \"players must only use their left hand\"",
    "Take a drink and then the player on your left must take a drink and then the player on their right and then so on until it is back to you",
    "Take a drink from every players drink",
    "Choose heads or tails and then flip a coin, if correct then everyone except you drinks, if incorrect then you drink",
    "You choose which player is best dressed, they must drink",
    "You must not speak English for one round",
    "You can pick a word that is banned for {1}, if anyone says this word they must drink",
    "Take a selfie with the player on your right and then both drink",
    "Everyone down their drinks",
    "If your drink is mixed then take a drink",
    "The player with the longest hair must drink",
    "Shake hands with every player",
    "Choose a player to remove a piece of clothing",
    // Killer's List
    // Truth Or Drink Questions
    "Have you ever lied in Truth or Dare?",
    "If you can be a girl/boy for a day, what would you do and why?",
    "Worst gift you have ever received?",
    "What's the weirdest thing you've ever eaten?",
    "What do you like most about everyone in the room?",
    "What's the most illegal thing you've ever done?",
    "What's something you've done while drunk that you would never do sober?",
    "Which of these questions would you be the most mortified to answer?",
    "Who's the most inappropriate person you've ever fantasized about?",
    "What's the most embarrassing thing you've ever done while drunk?",
    "When was the last time someone hit on you?",
    "How many people have you kissed?",
    "Have you ever been to a strip club?",
    "If you could make one wish right this second, what would it be?",
    // Random Stuff 
    "Call your crush and explain the rules of Monopoly to them",
    "Whenever someone says \"like\" you must say \"there we go again\" for {1}.",
    "Everything you say for the next {1} must be sung to the tune of \"Happy Birthday\".",
    "Pretend to be the person to your right for {1}.",
    "Talk without closing your mouth.",
    "Talk to a pillow like it's your crush.",
    "Have a full conversation with yourself in a mirror.",
    "What are four things you notice in a person at first glance?",
    "What makes you find someone attractive?",
    "What's the most disturbing fantasy or dream you've ever had?",
    "What's the most childish thing you still do?",
    "How old were you when your parents sat you down for \"the talk\" and what did they say (or not say) about \"the birds and the bees\"?",
    // Truth Or Drink Questions For Friends
    "What's the biggest secret you've ever kept from your parents?",
    "Of the people in this room, who do you most want to switch lives with and why?",
    "What do you like most and least about your own appearance?",
    "What do you like most and least about your personality?",
    "Tell a Drunken story.",
    "A fact of the last person you have kissed.",
    "Favorite TV show.",
    "What are you wearing right now?",
    "Favorite song.",
    "Last awkward situation you were last in?",
    "How many followers do you have?",
    "If you could erase one past experience, what would it be?",
    "What's the craziest thing you've ever done to attract a crush?",
    "Of the people in this room, who do you disagree with most frequently?",
    "What's your favorite go-to move for getting attention from the opposite sex?",
    // Yes Or No Questions 
    "Are flies and robins small insects?",
    "Are shoes worn over the top of socks?",
    "Are lasagna and spaghetti Chinese dishes?",
    "Are peanuts made into jam?",
    // GDoc Questions
    "Would you go out with an older woman?",
    "Would you make out with {0} for one minute?",
    "Have you ever fallen in love at first sight?",
    "What don't you like about {0}?",
    "What is the most illegal thing you have ever done?",
    "Who has the best dance moves?",
    "What's the worst date you've ever had?",
    "How many people in the room would you be willing to kiss?",
    "What's the weirdest dream you've ever had?",
    "What kind of underwear do you wear?",
    "What's your longest relationship?",
    "What is the biggest secret you've kept from your parents when you were growing up?",
    "Have you ever lied about being sick so you could stay home from work or school?",
    "What is your least favorite part about family gatherings?",
    "What is the one thing you dislike about yourself?",
    "If you could hire someone to do one thing for you, what would it be?",
    "What is your biggest pet peeve?",
    "What was the most embarrassing thing that you ever did while on a date?",
    "What was the last thing you searched for on your phone?",
    "Who do you think is the worst dressed person in this room?",
    "What color undergarments do you have on right now?",
    "If you lost one day of your life every time you said a swear word, would you stop?",
    "Who has been your best drinking buddy?",
    "What is your secret bad habit?",
    "Have you ever stalked someone via social media?",
    "Anyone who didn't drink last round, drink 2 times",
    "Bark every time someone says your name for {1}.",
    "Anyone that considers themselves a female, drink",
    "Choose a drinking buddy, drink together every time",
    "Tell everyone the story of the worst lie you ever told or drink 2 times",
    "Point at the person with the lowest IQ or drink 2 times",
    "Point at the person with the highest IQ or drink 2 times",
    "Did you have an imaginary friend growing up?",
    "Did your parents ever give you the 'birds and the bees' talk?",
    "What would be in your web history that you'd be embarrassed if someone saw?",
    "Do you drool in your sleep?",
    "Do you sleep with a stuffed animal?",
    "Who do you like the least in this room, and why?",
    "Have you ever tasted your sweat?",
    "If you were allowed to marry more than one person, would you? Who would you choose to marry?",
    "Would you rather live with no internet or no A/C or heating?",
    "Have you ever been caught checking someone out?",
    "What is your biggest fear?",
    "Would you wear your shirt inside out for a whole day if someone paid you $100?",
    "Tell us about a time you embarrassed yourself in front of a crush.",
    "What's the most useless piece of knowledge you know?",
    "Who do you think is the Beyonce of the group?",
    "Rate everyone in the room from 1 to 10, with 10 being the hottest.",
    "Rate everyone in the room from 1 to 10, with 10 being the best personality.",
    "Do you currently have a crush on anyone?",
    "What is your crush's personality like?",
    "Who is your biggest celebrity crush?",
    "Who do you think is the hottest in our group?",
    "If a girl you didn't like had a crush on you, how would you act around her?",
    "If we formed a boy band, who would make the best lead singer?",
    "Are you still a virgin?",
    "Who's the most annoying person in this room?",
    "Do you want to have kids? How many?",
    "Have you ever failed a class?",
    "Who do you want to make out with the most?",
    "Who has the best smile?",
    "Who here do you think is the best flirt?",
    // Drink If
    "Drink if: You're definitely going to be hungover tomorrow!",
    "Drink if: You're still only on your first drink",
    "Drink if: You're still sober!",
    "Drink if: You're drinking wine",
    "Drink if: You're drinking beer",
    "Drink if: You've never had a bad hangover",
    "Drink if: You're a smoker",
    "Drink if: You still get ID'd at the supermarket/pub/bars",
    "Drink if: You can speak more than one language",
    "Drink if: You're the tallest in the room",
    "Drink if: You're the shortest in the room",
    "Drink if: You've never driven a car",
    "Drink if: You went to university",
    "Drink if: You're the youngest in the room",
    "Drink if: You've got a tattoo",
    "Drink if: You're single",
    "Drink if: You've got more than one piercing",
    "Drink if: You're the last person to go to the bathroom!",
    "Drink if: You've ever been to a strip club",
    "Drink if: You've acted as a wingman before",
    "Drink if: You've been drunk in an Uber",
    "Drink if: You've ever gotten high",
    "Drink if: You have lied during this game",
    "Drink if: You've grabbed an electric fence.",
    // More Random assortment
    "What's the #1 thing you would never want your parents to find out about you?",
    "Say something you've never said before.",
    "Give someone in the room some words of advice.",
    "Drink and do something you wouldn't do while sober",
    "{0}, {2}",
    "Kiss {0}, or drink",
    "Drink then compliment {0}",
    "Drink then insult {0}",
    "Stare at {0} for one round. If you fail: {2}",
  };
  
  public void Start() {
    Debug.Log(questions.Length + " Total Questions loaded (DrunkCards)");
    roomBounds = new Bounds(center + transform.position, size);
    masterToggle.isOn = masterLocked;
    if (Networking.LocalPlayer.isMaster) {
      currentUsername = Networking.LocalPlayer.displayName;
      currentVersion = myVersion;
      usernameText.text = currentUsername;
      RequestSerialization();
    }
    UpdateTextBox(currentText);
  }
  public override void OnDeserialization() {
    if (masterToggle.isOn != masterLocked) {
      Debug.Log("DrunkCards Master Lock: " + masterLocked + " (am master: " + Networking.LocalPlayer.isMaster + ")");
    }
    masterToggle.isOn = masterLocked;
    usernameText.text = currentUsername;
    UpdateTextBox(currentText);
  }
  public void ClickedRandom() {
    if (masterLocked && !Networking.LocalPlayer.isMaster) return;
    if (!Networking.IsOwner(gameObject)) {
      Networking.SetOwner(Networking.LocalPlayer, gameObject);
    }
    previousText = currentText = Random.Range(0, questions.Length);
    currentVersion = myVersion;
    currentUsername = Networking.LocalPlayer.displayName;
    UpdateTextBox(currentText);
    RequestSerialization();
  }
  public void ClickedClear() {
    if (masterLocked && !Networking.LocalPlayer.isMaster) return;
    if (!Networking.IsOwner(gameObject)) {
      Networking.SetOwner(Networking.LocalPlayer, gameObject);
    }
    textBox.text = "";
    previousText = currentText = -2;
    currentUsername = Networking.LocalPlayer.displayName;
    RequestSerialization();
  }
  public void ToggleMasterLock() {
    Debug.Log("Toggling Master Lock: I am Master (" + Networking.LocalPlayer.isMaster + "), currently (" + masterLocked + ")");
    if (!Networking.LocalPlayer.isMaster) {
      if (masterToggle.isOn != masterLocked) masterToggle.isOn = masterLocked;
      return;
    }
    if (!Networking.IsOwner(gameObject)) {
      Networking.SetOwner(Networking.LocalPlayer, gameObject);
    }
    masterLocked = !masterLocked;
    Debug.Log("Toggling Master Lock Updated to (" + masterLocked + ")");

    RequestSerialization();
    if (clickSound != null) clickSound.PlayOneShot(clickSound.clip, 1.0f);
  }

  private void UpdateTextBox(int currentText) {
    string newText;
    if (currentText >= 0) {
      if (currentVersion != myVersion) {
        Debug.Log($"DrunkCards version mismatch (Mine: {myVersion}, current: {currentVersion})");
        textBox.text = $"Your game version does not match {currentUsername}. You will only be able to see prompts from people on the same version as you";
        return;
      }
      if (previousText != currentText) {
        Debug.Log("DrunkCards updated: " + currentText + " " + (currentText < 0 ? "" : questions[currentText]));
      }
      newText = questions[currentText];
    } else if (currentText == -1) {
      newText = "Click Random or take a shot.";
    } else {
      newText = "";
    }

    if (Networking.IsOwner(gameObject)) {
      if (newText.Contains("{0}")) {
        VRCPlayerApi[] players = PickPlayersInProximity(1);
        int len = players.Length;
        template0 = "";
        if (len == 0) template0 = "someone";
        for (int i = 0; i < len; ++i) {
          template0 += players[i].displayName;
          if (i < len - 1) template0 += ", ";
          if (i > 0 && i == len - 2) template0 += "and ";
        }
      }
      if (newText.Contains("{1}")) {
        int num = GetWeightedRand();
        template1 = $"{num} round";
        if (num != 1) template1 += "s";
      }
      if (newText.Contains("{2}")) {
        int num = GetWeightedRand();
        template2 = $"{num} drink";
        if (num != 1) template2 += "s";
      }
    }
    newText = string.Format(newText, template0, template1, template2);

    textBox.text = newText;
  }
  private int GetWeightedRand() {
    float index = Random.value;
    float sum = 0;
    int num = 1;
    for (int i = 0; i < weights.Length; ++i) {
      if ((sum += weights[i]) <= index) {
        num = i + 1;
      } else {
        break;
      }
    }
    return num;
  }

  private VRCPlayerApi[] PickPlayersInProximity(int count) {
    VRCPlayerApi.GetPlayers(playerBuf);
    int numInProximity = 0;

    foreach (VRCPlayerApi player in playerBuf) {
      if (player == null) continue;
      if (player.isLocal) continue;
      Vector3 pos = player.GetPosition();
      if (roomBounds.Contains(pos)) {
        playerBuf2[numInProximity++] = player;
      }
    }

    count = Mathf.Min(count, numInProximity);
    VRCPlayerApi[] picked = new VRCPlayerApi[count];

    for (; count > 0; --count) {
      int rand = Random.Range(0, numInProximity);
      picked[count - 1] = playerBuf2[rand];
      for (int i = rand; i < numInProximity - 1; ++i) {
        playerBuf2[i] = playerBuf2[i + 1];
      }
      --numInProximity;
    }

    return picked;
  }

#if !COMPILER_UDONSHARP && UNITY_EDITOR
  void OnDrawGizmosSelected() {
    this.UpdateProxy(ProxySerializationPolicy.RootOnly);
    Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
    Gizmos.DrawCube(center + transform.position, size);
    Gizmos.color = new Color(1f, 0f, 0f, 1f);
    Gizmos.DrawWireCube(center + transform.position, size);
  }
#endif
}
