# GruppoReti.DAL

this is an Entity Framework 6.x generic repository that uses:
1. UnitOfWork Pattern
2. DbContext per Request: this means that each request (http or not) is handled by creating a single instance of the DbContext
3. Entities generated into a separated project

The solution contains 3 projects:

1. GruppoReti.DAL --> the project containig the edmx and the classes that implements the repository pattern
2. GruppoReti.Entities --> the project containig the entities mapped from Database
3. GruppoReti.Managers --> the BL layer that uses the DAL

inside the Managers project there are some usage examples (work in progress please be patient.....)

BASIC USAGE
===========
inside your manager class create a repository instance writing:
```c#
EFRepository<Entitiy_Name>  Repo = new EFRepository<Entity_Name>();
```
now in the Repo object you'll find all the methods needed to perform queries and other operations (INS, UPD, DEL)
```c#
Repo.Add
Repo.AddAll
Repo.Find
Repo.IncludeMultiple --> to include into the query result the nested entities because this project uses the eager loading pattern

Repo.Update
Repo.UpdateAll
Repo.Delete
Repo.DeleteAll
```
