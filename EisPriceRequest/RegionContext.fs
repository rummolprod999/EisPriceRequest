namespace ParserFsharp
open Microsoft.EntityFrameworkCore
open System
open System.ComponentModel.DataAnnotations.Schema

type Region() =
    [<property: Column("id")>]
    member val public Id = 0 with get, set

    [<property: Column("okrug_id")>]
    member val public OkrugId = 0 with get, set

    [<property: Column("name")>]
    member val public Name = "" with get, set

    [<property: Column("path")>]
    member val public Path = "" with get, set

    [<property: Column("conf")>]
    member val public Conf = "" with get, set

    [<property: Column("path223")>]
    member val public Path223 = "" with get, set

type RegionContext() =
    inherit DbContext()

    [<DefaultValue>]
    val mutable regions : DbSet<Region>
    member x.Regions
        with get () = x.regions
        and set v = x.regions <- v

    override __.OnConfiguring(optbuilder : DbContextOptionsBuilder) =
        optbuilder.UseMySQL(S.Settings.ConS) |> ignore
        ()

    override __.OnModelCreating(modelBuilder : ModelBuilder) =
         base.OnModelCreating(modelBuilder)
         modelBuilder.Entity<Region>().ToTable(String.Format("{0}region", S.Settings.Pref)) |> ignore
         ()


