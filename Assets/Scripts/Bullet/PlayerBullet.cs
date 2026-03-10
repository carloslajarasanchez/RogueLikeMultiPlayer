using UnityEngine;
using Unity.Netcode;

public class PlayerBullet : Projectile
{
    protected override void OnCollisionEnter(Collision other)
    {
        Enemy target = other.gameObject.GetComponent<Enemy>();
        if (target != null)
        {
            NetworkObject netObj = target.GetComponent<NetworkObject>();
            if (netObj != null)
                RequestDamageServerRpc(netObj.NetworkObjectId);

            Deactivate();
            return;
        }
        Deactivate();
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestDamageServerRpc(ulong targetNetworkId)
    {
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects
            .TryGetValue(targetNetworkId, out NetworkObject netObj)) return;

        Enemy target = netObj.GetComponent<Enemy>();
        if (target != null)
            target.TakeDamage();
    }
}