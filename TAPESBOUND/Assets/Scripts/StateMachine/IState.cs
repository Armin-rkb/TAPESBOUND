public interface IState
{
    // Start is called before the first frame update
    void OnStateEnter();
    
    // Update is called once per frame
    void OnStateTick();

    // Update is called once per frame
    void OnStateExit();

}
