# CollectCruiserItemCompany

A Lethal Company mod that collects items inside the cruiser into the ship floor on the Company and in some limited situations.

## What it does

- Cruiser Item Collection by Terminal

## When it activates

Cruiser item collection feature activates when any of these conditions are met:

- While landed on the Company
- In orbit on the first day (also the day after ejected)
- In orbit on the next day after landing on the Company, still routing to the Company (in the same session only)

## Who needs to install

Host only; clients are not required. However, only clients with this mod installed can trigger the collection.

Item positions are synchronized for all players, even if they don't have this mod installed.

## Configuration

| Name | Type | Default | Description |
|:--------|:-----|:--------|:------------|
| Permission | enum | HostOnly | Controls who can collect items from the cruiser. If HostOnly, only the host can collect items. If Everyone, all players can collect items if they have installed this mod. |

## FAQ

### Log shows some ThrowObjectClientRpc errors when collecting. Is this a problem?

No. These errors are expected because this mod teleports items using the vanilla method to drop items by players.

The internal game logic detects an invalid state that an item is not held by a player when it is dropped, so it logs errors.

However, this may become an issue in future game versions.

```plain
[Info   :CollectCruiserItemCompany] Teleporting item. name=FancyGlass(Clone) worldOldItemPosition=(0.02, 5.60, -21.01) localNewItemPosition=(3.05, 0.17, -4.72) worldNewItemPosition=(4.33, 0.45, -12.23)
[Error  : Unity Log] ThrowObjectClientRpc called for an object which is not the same as currentlyHeldObjectServer which is null, on player #0.
```

## Differences from [4902/Cruiser_Additions](https://thunderstore.io/c/lethal-company/p/4902/Cruiser_Additions/)

`4902/Cruiser_Additions` can collect items when the ship is going into orbit.

This mod can collect items only when the ship is on the Company and some limited situations.

`4902/Cruiser_Additions` also includes other features such as additional scraps, a speedometer, and more.

This mod focuses solely on the item-collection feature.
