# Entitlements
Unity supports the PlayStation 5 (PS5) [Entitlements](https://p.siedev.net/resources/documents/WebAPI/1/Entitlements-Overview/0001.html) system as part of the PlayStation Network (PSN) package. 

An entitlement is a rule granting conditional access for a user to protected resources associated with a product. Entitlements can include:

* Titles
* Downloadable Content
* Demos
* Themes

You can grant the user Entitlements on the following bases:

|**Type**|**Description**|
|---|---|
|**Date**|Choose when access begins and ends with a date.|
|**Limited use**|Choose a specific number of times a user can access the entitlement.|
|**Permanent access**|The user has constant access to the entitlement.|

Entitlements can be both retrieved and updated, allowing you to update single entitlements using an entitlement ID or retrieve a complete list of a user's available entitlements.

It's recommended to check entitlement validity at the following times:
* On the launch of the title.
* When a user selects a specific menu within the title.
* When a user buys a certain item or content within the title.
* When an entitlement update occurs.

## Unified entitlements
A Unified Entitlement is a product element that gives certain privileges to the user. For example, it can be a title or game content like an in-game item.

Each unified entitlement has a package type, which identifies what the entitlement does. The types available for PS5 are:

### Managed in PS5 GEMS

|**Type**|**Description**|
|---|---|
|**PSGD**|PlayStation 5 game download.|
|**PSAC**|PlayStation 5 additional content.|
|**PSAL**|PlayStation 5 additional unlockable feature.|

### Managed in content pipeline

|**Type**|**Description**|
|---|---|
|**PSCONS**|PlayStation Store consumable (non-virtual currency).|
|**PSVC**|PlayStation Store virtual currency.|
|**PSSUBS**|PlayStation Store game developer subscription.|
|**PSIL**|PlayStation 5 individual license.|
|**PSTRACK**|PlayStation Store tracker.|

## Retrieve entitlement data

The [Entitlements API](xref:Unity.PSN.PS5.Entitlement) can retrieve entitlements held by the current user. Entitlements work alongside the Commerce API when users buy game content, with entitlements managing access limits to the content.

Further information on entitlements is available on [Sony Devnet](https://p.siedev.net/resources/documents/WebAPI/1/Entitlements-Overview/0001.html).

**Note**: You must set up a store for each region you intend to make entitlements available for.

