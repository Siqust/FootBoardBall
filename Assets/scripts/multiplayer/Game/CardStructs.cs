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
    RandomCard, //���������! ������ ����� �� ���������� ��������� ����� �� ������. 
    WeatherChild, //���� ������! ������ ������ �� ������.
    Halk, //����! ������ �� ���� ����� �������� ������� �� ���� ��������� ������ ����.
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