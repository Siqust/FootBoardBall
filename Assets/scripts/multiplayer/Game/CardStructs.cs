using System;


public enum CardType : byte
{
    None,
    Player,
    ActPlayer,
    ActRow,
    ActField
}
public enum CardScriptType : byte
{
    ScriptableObject,
    Playercard,
    ActionCard
}

public enum PlayerAbility : byte
{
    None,
    RandomCard, //Ваббаджек! Выходя сразу же заигрывает случайную карту из колоды. 
    WeatherChild, //Дитя погоды! Бонусы погоды не влияют.
    Halk, //Халк! Выходя на поле может случайно вывести из игры соперника слабее себя.
    Finter,
    IronMan,
    Fearless,
    favorite,
    Volya, // TODO
    CURASH, //TODO
    Mascot,
    Guardian,
    Spy,
    Cannon,
    Star,
    Leprechaun,
    Leader,
    Bulldozer,
    Decisive,
    Replicator,
    Siren
}