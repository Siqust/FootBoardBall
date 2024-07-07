using System;


public enum ActionCardType : byte
{
    None,
    ActPlayerLT,
    ActPlayerOT,
    ActRow,
    ActField
}
public enum RowType : byte
{
    Attack,
    Defence
}
public enum MoveType : byte
{
    Player,
    ActionOnPlayer,
    ActionOnRow
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

public enum ActionAbility : byte
{
    None,
    RemovePlayer
}