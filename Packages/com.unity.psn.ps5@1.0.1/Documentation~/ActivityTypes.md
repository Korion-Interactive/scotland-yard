# Activity Types

The following table describes the types of Activities that are available. After you have decided which types of Activity you want to include in your game, use the [UDS Management tool](https://tools.partners.playstation.net/uds/app/uds-management/dashboard?sid=26&pid=psn) to create and configure them. 


| **Activity Type** | **Description** |
| --- | --- |
| Progress | Any Activity that requires the player to complete a task or set of tasks. |
| Open-Ended  | Has no specific completion objective. It ends when the player chooses to end it. |
| Competitive | Determines a ranking based on the outcome of synchronous play experiences. <br/> Competitive activities also have the Matches feature, which show up as cards and enable other players to join in. Matches also have their own API. For more information about Matches, see [Matches Web API Overview](https://p.siedev.net/resources/documents/WebAPI/1/Matches_WebAPI-Overview/__document_toc.html) on DevNet. |
| Challenge | A repeatable Activity that displays rankings in leaderboard format based on the outcome of asynchronous single-player results. |

Progress and Open-ended Activities can also include child Activities, such as Tasks and Sub-Tasks, that provide more granular objectives. For example, a Progress Activity “Complete All Levels” could have Tasks like “Complete Level 1” and “Complete Level 2”.

For more information about these Activity types, see [PSN Activities Technical Concepts](https://p.siedev.net/resources/documents/SDK/4.000/PSN_Activities_Service-Overview/0001.html#__document_toc_00000000).

To configure your activities for your Unity project, see [Set up and configure Activities](ConfigureActivities.md).