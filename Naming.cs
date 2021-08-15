using System;

namespace Utility.Naming
{
    /*
    각 네이밍 0~19번은 게임의 핵심적인 요소
    100~199번은 Room 0(UI Room)에서 사용되는 네이밍
    200~299번은 room 1에서 사용되는 네이밍
    300~399번은 room 2에서 사용되는 네이밍
    400~499번은 room 3에서 사용되는 네이밍
    500번 이후는 게임의 핵심적인 요소는 아니나, Room별로 Sharing 가능한 네이밍
    을 사용하도록 한다
    */

    public enum SoundNaming
    {
        NullSound = 0,
        BackGroundMusic_1 = 1,
        TestSound = 19,
    }
}
