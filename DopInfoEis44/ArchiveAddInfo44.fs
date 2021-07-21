namespace ParserFsharp

open Microsoft.EntityFrameworkCore
open System
open System.ComponentModel.DataAnnotations.Schema
open System.ComponentModel.DataAnnotations

type ArchiveAddInfo44() =
    [<Key>]
    [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
    [<property: Column("id")>]
    member val public Id = 0 with get, set

    [<property: Column("archive")>]
    member val public Archive = "" with get, set

    [<property: Column("archive_size")>]
    member val public SizeArch = 0 with get, set

type ArchiveAddInfo44Context() =
    inherit DbContext()

    [<DefaultValue>]
    val mutable archives : DbSet<ArchiveAddInfo44>
    member x.Archives
        with get () = x.archives
        and set v = x.archives <- v

    override __.OnConfiguring(optbuilder : DbContextOptionsBuilder) =
        optbuilder.UseMySQL(S.Settings.ConS) |> ignore
        ()

    override __.OnModelCreating(modelBuilder : ModelBuilder) =
         base.OnModelCreating(modelBuilder)
         modelBuilder.Entity<ArchiveAddInfo44>().ToTable(String.Format("{0}archive_add_info44", S.Settings.Pref)) |> ignore
         ()
