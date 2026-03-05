using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    // Referencias a las dos visagras que tienen DoorsMove
    [SerializeField] private DoorsMove _doorLeft;
    [SerializeField] private DoorsMove _doorRight;

    // La sala a la que pertenece esta puerta
    // Se asigna automáticamente desde RoomController
    private RoomController _myRoom;

    public void Init(RoomController room)
    {
        _myRoom = room;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (_myRoom == null) return;
        _myRoom.RequestDoorOpen(this);
    }

    public void OpenDoor(bool inward = false)
    {
        if (_doorLeft != null)
        {
            if (inward) _doorLeft.OpenInward();
            else _doorLeft.OpenOutward();
        }
        if (_doorRight != null)
        {
            if (inward) _doorRight.OpenInward();
            else _doorRight.OpenOutward();
        }
    }

    public void CloseDoor()
    {
        if (_doorLeft != null) _doorLeft.CloseDoor();
        if (_doorRight != null) _doorRight.CloseDoor();
    }
}