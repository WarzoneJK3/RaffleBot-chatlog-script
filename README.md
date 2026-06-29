# Raffle Logs

TL;DR: Check [PlayerData.tsv](/Files/PlayerData.tsv) and [RaffleData.tsv](/Files/RaffleData.tsv).

## About this project

This repository contains the majority of Raffles since 2021-04-13 14:33:31 UTC and goes until I get tired of updating it.<br>
It allows for extracting a variety of Raffle data, and this project also has some code to do basic parsing of the chat log.<br>
It currently does not do further statistics beyond basic data extraction and totalization.

Dates are in the format yyyy-mm-dd and times are UTC in hh:mm:ss format.<br>
Note that players are identified purely by the first part of their username (as used by RaffleBot), and that names might change over time.

## What is where?

- [RaffleBot chat log.txt](/RaffleBot%20chat%20log.txt) is the raw chat log of the RaffleBot.
- The [Files](/Files/) directory has pre-parsed data in tab-separated-value format. You can uses these files to perform your own data analysis without having to bother with extracting the data from the raw log file:
  - [Data.tsv](/Files/Data.tsv) holds a parsed version of every message in the log;
  - [PlayerData.tsv](/Files/PlayerData.tsv) holds data per player, including total coins and raffles won and joined;
  - [RaffleData.tsv](/Files/RaffleData.tsv) holds data per raffle, including its coin value and who won it. 
- The [RaffleLogParser](/RaffleLogParser/) directory has a C# project inside of it which can be used to parse the raw chat log.

## When was this last updated?

You can check the date and time of the last Github commit to see when it was last updated.

## Contributing

If you would like to contribute, consider adding or suggesting:
- Visualisation for the data
- Interesting statistics based on data

I'm also open to other suggestions and contributions, but don't get upset if I ignore or decline it...

## Contact
This project is semi-maintained by [JK_3](https://war.app/Profile?p=31105111944). 
Reach out on (preferably) Discord or otherwise via WarApp mail if you need to contact me.
