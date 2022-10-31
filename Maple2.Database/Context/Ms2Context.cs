﻿using Maple2.Database.Model;
using Microsoft.EntityFrameworkCore;

namespace Maple2.Database.Context;

public sealed class Ms2Context : DbContext {
    internal DbSet<Account> Account { get; set; } = null!;
    internal DbSet<Character> Character { get; set; } = null!;
    internal DbSet<CharacterConfig> CharacterConfig { get; set; } = null!;
    internal DbSet<CharacterUnlock> CharacterUnlock { get; set; } = null!;
    internal DbSet<Home> Home { get; set; } = null!;
    internal DbSet<Item> Item { get; set; } = null!;
    internal DbSet<PetConfig> PetConfig { get; set; } = null!;
    internal DbSet<ItemStorage> ItemStorage { get; set; } = null!;
    internal DbSet<Club> Club { get; set; } = null!;
    internal DbSet<ClubMember> ClubMember { get; set; } = null!;
    internal DbSet<SkillTab> SkillTab { get; set; } = null!;
    internal DbSet<Buddy> Buddy { get; set; } = null!;
    internal DbSet<UgcMap> UgcMap { get; set; } = null!;
    internal DbSet<UgcMapCube> UgcMapCube { get; set; } = null!;
    internal DbSet<Mail> Mail { get; set; } = null!;
    internal DbSet<MesoListing> MesoMarket { get; set; } = null!;
    internal DbSet<SoldMesoListing> MesoMarketSold { get; set; } = null!;

    public Ms2Context(DbContextOptions options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Account>(Maple2.Database.Model.Account.Configure);
        modelBuilder.Entity<Character>(Maple2.Database.Model.Character.Configure);
        modelBuilder.Entity<CharacterConfig>(Maple2.Database.Model.CharacterConfig.Configure);
        modelBuilder.Entity<CharacterUnlock>(Maple2.Database.Model.CharacterUnlock.Configure);
        modelBuilder.Entity<Home>(Maple2.Database.Model.Home.Configure);
        modelBuilder.Entity<Item>(Maple2.Database.Model.Item.Configure);
        modelBuilder.Entity<PetConfig>(Maple2.Database.Model.PetConfig.Configure);
        modelBuilder.Entity<ItemStorage>(Maple2.Database.Model.ItemStorage.Configure);
        modelBuilder.Entity<Club>(Maple2.Database.Model.Club.Configure);
        modelBuilder.Entity<ClubMember>(Maple2.Database.Model.ClubMember.Configure);
        modelBuilder.Entity<SkillTab>(Maple2.Database.Model.SkillTab.Configure);
        modelBuilder.Entity<Buddy>(Maple2.Database.Model.Buddy.Configure);
        modelBuilder.Entity<UgcMap>(Maple2.Database.Model.UgcMap.Configure);
        modelBuilder.Entity<UgcMapCube>(Maple2.Database.Model.UgcMapCube.Configure);
        modelBuilder.Entity<Mail>(Maple2.Database.Model.Mail.Configure);

        modelBuilder.Entity<MesoListing>(MesoListing.Configure);
        modelBuilder.Entity<SoldMesoListing>(SoldMesoListing.Configure);
    }
}
