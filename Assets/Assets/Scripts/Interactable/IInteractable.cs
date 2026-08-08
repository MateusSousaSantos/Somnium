// Anything the player's proximity check (PlayerInteractor) can interact with via E -
// world containers today, doors/switches/NPCs later. No range concept here; that's
// entirely PlayerInteractor's job (it picks the closest IInteractable within its own
// check radius), so implementers just react to Interact() being called.
public interface IInteractable
{
    string DisplayName { get; }
    void Interact();
}
