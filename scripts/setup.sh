#!/bin/bash
# ============================================================
# AutoPartsHub — Setup inicial do repositório
# Execute uma vez após clonar: bash scripts/setup.sh
# ============================================================

set -e

echo "🚗 AutoPartsHub — Setup inicial"
echo "================================"

# Verificar dependências
check_cmd() {
  if ! command -v "$1" &> /dev/null; then
    echo "❌ $1 não encontrado. Instale antes de continuar."
    exit 1
  fi
}

check_cmd dotnet
check_cmd node
check_cmd npm
check_cmd docker
check_cmd git

echo "✅ Dependências verificadas"

# Instalar ferramentas globais .NET
echo "📦 Instalando ferramentas .NET..."
dotnet tool install --global dotnet-ef 2>/dev/null || dotnet tool update --global dotnet-ef
dotnet tool install --global csharpier 2>/dev/null || true

# Instalar commitlint + husky no raiz
echo "📦 Instalando commitlint e husky..."
npm init -y --silent 2>/dev/null || true
npm install --save-dev \
  @commitlint/cli \
  @commitlint/config-conventional \
  husky \
  --silent

# Configurar Husky
echo "🐶 Configurando Husky..."
npx husky install

# Hook: validar mensagem de commit
npx husky add .husky/commit-msg 'npx --no -- commitlint --edit "$1"'

# Hook: lint e format antes do commit
cat > .husky/pre-commit << 'EOF'
#!/bin/sh
. "$(dirname "$0")/_/husky.sh"

echo "🔍 Verificando formatação .NET..."
dotnet csharpier --check . 2>/dev/null || {
  echo "❌ Código .NET fora do padrão. Rode: dotnet csharpier ."
  exit 1
}
EOF
chmod +x .husky/pre-commit

# Subir infraestrutura local
echo "🐳 Subindo PostgreSQL e Redis..."
docker compose up -d postgres redis

echo ""
echo "✅ Setup concluído!"
echo ""
echo "Próximos passos:"
echo "  1. Copie appsettings.Example.json → appsettings.Development.json e preencha"
echo "  2. dotnet ef database update --project src/AutoPartsHub.Infra --startup-project src/AutoPartsHub.API"
echo "  3. dotnet run --project src/AutoPartsHub.API"
echo ""
echo "Secrets necessários no GitHub (Settings → Secrets):"
echo "  - AWS_ACCOUNT_ID"
echo "  - AWS_ACCESS_KEY_ID"
echo "  - AWS_SECRET_ACCESS_KEY"
echo "  - STAGING_DB_CONNECTION"
echo "  - ANTHROPIC_API_KEY"
