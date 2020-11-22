public interface IInteractable
{
    void Interact();
}

public interface IMove
{
    void Move();
}

public interface IQuest
{
    void AcceptQuest();
    void Complete();
}

public interface IItem
{
    void Use();
}
