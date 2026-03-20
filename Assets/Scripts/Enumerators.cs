namespace Enumerators {

    public enum GuardState {
        Patrol,
        Chase,
        Stop,
        Thinking
    }

    public enum GuardNotificationMessage {
        Player_Visible,
        Player_Not_Visible,
        Player_Caught,
    }

    public enum GameState {
        Player_Being_Chased,
        Player_Juked_Chased,
        Player_Got_Caught,
    }

}