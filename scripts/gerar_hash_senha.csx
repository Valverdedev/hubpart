#!/usr/bin/env dotnet-script
// Gera o password_hash do ASP.NET Identity para uso no seed SQL.
// Pré-requisito: dotnet tool install -g dotnet-script
//
// Uso:
//   dotnet script scripts/gerar_hash_senha.csx -- "SuaSenha@123"
//
// Cole o hash gerado no campo password_hash do seed_usuario_admin.sql

#r "nuget: Microsoft.AspNetCore.Identity, 2.3.1"

using Microsoft.AspNetCore.Identity;

var senha = Args.Count > 0 ? Args[0] : "Admin@123456";
var hasher = new PasswordHasher<object>();
var hash = hasher.HashPassword(new object(), senha);

Console.WriteLine($"Senha : {senha}");
Console.WriteLine($"Hash  : {hash}");
