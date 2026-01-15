#!/bin/bash
# release.sh
# Kullanım: ./release.sh 1.0.1 "Yaptığım değişiklikler: bug fix ve yeni feature"

# Parametreleri al
VERSION=$1
COMMIT_MSG=$2

if [ -z "$VERSION" ]; then
  echo "Lütfen version numarasını girin. Örn: ./release.sh 1.0.1 \"Commit mesajı\""
  exit 1
fi

# package.json versiyonunu değiştir
# Bu satır package.json içindeki "version": "..." kısmını günceller
sed -i.bak -E "s/\"version\": \"[0-9]+\.[0-9]+\.[0-9]+\"/\"version\": \"$VERSION\"/" package.json

# Değişikliği stage et
git add package.json

# Commit oluştur
if [ -z "$COMMIT_MSG" ]; then
  git commit -m "Update package version to $VERSION"
else
  git commit -m "Update package version to $VERSION: $COMMIT_MSG"
fi

# Branch'i push et (main kullanıyorsan)
git push origin main

# Tag oluştur
git tag -a "v$VERSION" -m "Release $VERSION"

# Tag'i push et
git push origin "v$VERSION"

echo "✅ Paket $VERSION sürümü ile güncellendi ve GitHub'a push edildi."
