namespace ParserFsharp

open Microsoft.EntityFrameworkCore
open System
open System.ComponentModel.DataAnnotations.Schema
open System.ComponentModel.DataAnnotations
open Microsoft.Extensions.Logging

type ArchivePriceRequest44() =
    [<Key>]
    [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
    [<property: Column("id")>]
    member val public Id = 0 with get, set

    [<property: Column("archive")>]
    member val public Archive = "" with get, set

    [<property: Column("archive_size")>]
    member val public SizeArch = 0 with get, set

type ArchivePriceRequest44Context() =
    inherit DbContext()

    [<DefaultValue>]
    val mutable archives : DbSet<ArchivePriceRequest44>
    member x.Archives
        with get () = x.archives
        and set v = x.archives <- v

    override __.OnConfiguring(optbuilder : DbContextOptionsBuilder) =
        //optbuilder.UseLoggerFactory(LoggerFactory.Create(fun builder -> builder.AddConsole() |> ignore)) |> ignore
        optbuilder.UseMySQL(S.Settings.ConS) |> ignore
        ()

    override __.OnModelCreating(modelBuilder : ModelBuilder) =
         base.OnModelCreating(modelBuilder)
         modelBuilder.Entity<ArchivePriceRequest44>().ToTable(String.Format("{0}archive_price_request44", S.Settings.Pref)) |> ignore
         ()
