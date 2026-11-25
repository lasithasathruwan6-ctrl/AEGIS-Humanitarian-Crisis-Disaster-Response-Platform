using System.Security.Cryptography;
using CrisisPlatform.Api.Common.Security.Nic;

namespace CrisisPlatform.Tests.Security;

public class NicHashTests
{
    private sealed class InMemoryNicIndex : INicIndex
    {
        public HashSet<string> Hashes { get; } = new();

        public Task<bool> TryAddAsync(string nicHash, CancellationToken cancellationToken = default)
            => Task.FromResult(Hashes.Add(nicHash));
    }

    private readonly byte[] _key = RandomNumberGenerator.GetBytes(32);

    [Fact]
    public void Hash_is_deterministic_for_the_same_key_and_nic()
    {
        var hasher = new NicHasher(_key);

        Assert.Equal(hasher.Hash("853202345V"), hasher.Hash("853202345V"));
    }

    [Fact]
    public void Hash_does_not_contain_or_equal_the_plain_nic()
    {
        var hash = new NicHasher(_key).Hash("853202345V");

        Assert.DoesNotContain("853202345", hash);
        Assert.Equal(64, hash.Length);
    }

    [Fact]
    public void Hash_depends_on_the_secret_key()
    {
        var other = new NicHasher(RandomNumberGenerator.GetBytes(32));

        Assert.NotEqual(new NicHasher(_key).Hash("853202345V"), other.Hash("853202345V"));
    }

    [Fact]
    public void Different_nics_produce_different_hashes()
    {
        var hasher = new NicHasher(_key);

        Assert.NotEqual(hasher.Hash("853202345V"), hasher.Hash("853202346V"));
    }

    [Theory]
    [InlineData("853202345v")]
    [InlineData(" 853202345V ")]
    public void Case_and_spacing_do_not_change_the_hash(string variant)
    {
        var hasher = new NicHasher(_key);

        Assert.Equal(hasher.Hash("853202345V"), hasher.Hash(variant));
    }

    [Fact]
    public void Old_and_new_format_of_the_same_person_share_one_hash()
    {
        var hasher = new NicHasher(_key);

        Assert.Equal(hasher.Hash("853202345V"), hasher.Hash("198532002345"));
    }

    [Fact]
    public async Task Registering_the_same_nic_twice_is_refused()
    {
        var service = new NicUniquenessService(new InMemoryNicIndex(), new NicHasher(_key));

        Assert.True(await service.TryRegisterAsync("853202345V"));
        Assert.False(await service.TryRegisterAsync("853202345V"));
        Assert.False(await service.TryRegisterAsync("853202345x"));
    }

    [Fact]
    public async Task Uniqueness_also_blocks_the_other_format_of_the_same_nic()
    {
        var service = new NicUniquenessService(new InMemoryNicIndex(), new NicHasher(_key));

        Assert.True(await service.TryRegisterAsync("853202345V"));
        Assert.False(await service.TryRegisterAsync("198532002345"));
    }

    [Fact]
    public async Task Index_holds_only_hashes_never_plain_nics()
    {
        var index = new InMemoryNicIndex();
        var service = new NicUniquenessService(index, new NicHasher(_key));

        await service.TryRegisterAsync("853202345V");

        Assert.DoesNotContain("853202345V", index.Hashes);
        Assert.All(index.Hashes, h => Assert.DoesNotContain("853202345", h));
    }

    [Fact]
    public void Invalid_nic_and_short_key_are_refused()
    {
        Assert.Throws<ArgumentException>(() => new NicHasher(_key).Hash("bad"));
        Assert.Throws<ArgumentException>(() => new NicHasher(new byte[8]));
    }
}
