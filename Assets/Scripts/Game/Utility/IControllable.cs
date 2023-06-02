using System.Collections;

public interface IControllable 
{
    public IEnumerator Controll();

    public void Up() { }
    public void Down() { }
    public void Right() { }
    public void Left() { }
    public void Wait() { }
    public void TurnMode() { }
    public void Submit() { }
    public void Cancel() { }
    public void Menu() { }
}
