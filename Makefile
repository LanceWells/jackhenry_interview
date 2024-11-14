encode-env:
	echo Run the following: gpg --batch -c --passphrase PASSPRHASE_HERE ./.env
reveal-env:
	gpg -d ./.env.gpg > .env
