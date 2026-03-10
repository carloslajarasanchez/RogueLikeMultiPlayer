using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    [HideInInspector] public int NetworkDoorId = -1;

    private RoomController _room;
    private DoorsMove[] _doorsMoves; // Array en lugar de uno solo

    public void Init(RoomController room)
    {
        _room = room;
        _doorsMoves = GetComponentsInChildren<DoorsMove>(); // todos los hijos
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (_room != null)
            _room.RequestDoorOpen(this);
    }

    public void OpenDoor(bool enteringRoom)
    {
        if (_doorsMoves == null) return;
        foreach (var door in _doorsMoves)
        {
            if (enteringRoom) door.OpenInward();
            else door.OpenOutward();
        }
    }

    public void CloseDoor()
    {
        if (_doorsMoves == null) return;
        foreach (var door in _doorsMoves)
            door.CloseDoor();
    }
}