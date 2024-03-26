# Trophies concepts and libraries

To include Trophies in your Unity project for PlayStation 5 (PS5), you must use the PlayStation Network package versions of the Sony SDK libraries and you must decide what types of trophies to create.

## SDK Libraries

There are two libraries in the PlayStation 5 SDK that interact with Trophies - *NpUniversalDataSystem* and *NpTrophy2*. The functions of these libraries are outlined in the following table:


| **Services** | **Description** |
| --- | --- |
| *NpUniversalDataSystem* Library | The *NpUniversalDataSystem* Library provides features that allow applications to unlock Trophies alongside updating Trophy progress and statistics by posting UDS events. <br/> For more information about initializing this library and posting UDS events for Trophies, see [ *NpUniversalDataSystem* Library Overview](https://p.siedev.net/resources/documents/SDK/5.000/NpUniversalDataSystem-Overview/__document_toc.html) and [Using the Library](https://p.siedev.net/resources/documents/SDK/5.000/NpUniversalDataSystem-Overview/0002.html) on DevNet. <br/> This library is represented in the PSN package by the *UniversalDataSystem* (UDS) C# class.|
| *NpTrophy2* Library | *NpTrophy2* acts as a library that allows applications to retrieve and store trophy configuration data and Trophy records. **This library does not influence the unlocking or updating of Trophies.** <br/> For more information about using this library to retrieve and store Trophy configuration data, see [*NpTrophy2* Library Overview](https://p.siedev.net/resources/documents/SDK/5.000/NpTrophy2-Overview/__toc.html). <br/> After starting the *NpTrophy2* library, you can register a callback function that receives notifications when Trophies are unlocked. For more information, see [sceNpTrophy2RegisterUnlockCallback](https://p.siedev.net/resources/documents/SDK/2.000/NpTrophy2-Reference/0027.html). <br/> This library is represented in the PSN package by the *TrophySystem* C# class.|


For more information about using these services, see [Game Play and the Trophy System](https://p.siedev.net/resources/documents/SDK/5.000/Trophy_System-Overview/0002.html) on DevNet.



## Trophy types

When configuring your own Trophies, there are two types you can implement:


| **Trophy type** | **Description** |
| --- | --- |
| Binary Trophy | Binary Trophies only have two states — locked and unlocked. A Binary trophy is unlocked when the value of the linked UDS Stat satisfies the trophy unlocking condition. <br/> Use binary Trophies to represent in-game achievements that the player completes with a single action. For example, use a binary trophy for an achievement at the end of a game level or mission. <br/> To unlock a binary trophy, you need only a single UDS event. Use `_UnlockTrophy` to send an unlock request when the player meets the required criteria for the trophy. |
| Progressive Trophy | Progressive Trophies measure a player’s continuous progress towards a goal. Use progressive Trophies to represent in-game achievements that the player completes incrementally. For example, use a progressive trophy to track collection of a target number of in-game items. <br/> To update progress, you can use `_UpdateTrophyProgress` each time the player completes an action towards the trophy. When the player reaches the target value, the trophy is automatically unlocked by UDS. You can also measure Trophy progress by declaring your own UDS Stats and Events. <br/> To manage progressive Trophies, create and update the Trophy’s statistics using the UDS Management Tool.|
