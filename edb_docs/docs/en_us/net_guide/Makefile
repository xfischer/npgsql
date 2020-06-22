# Minimal makefile for Sphinx documentation
#

# You can set these variables from the command line.
SPHINXOPTS    =
SPHINXBUILD   = sphinx-build
SPHINXPROJ    = ContainerTrial
SOURCEDIR     = .
BUILDDIR      = _build

all:	html epub latexpdf
	@echo finished

# Put it first so that "make" without argument is like "make help".
help:
	@$(SPHINXBUILD) -M help "$(SOURCEDIR)" "$(BUILDDIR)" $(SPHINXOPTS) $(O)

.PHONY: help Makefile

# Catch-all target: route all unknown targets to Sphinx using the new
# "make mode" option.  $(O) is meant as a shortcut for $(SPHINXOPTS).
%: Makefile
	@$(SPHINXBUILD) -M $@ "$(SOURCEDIR)" "$(BUILDDIR)" $(SPHINXOPTS) $(O)

html: Makefile
	@$(SPHINXBUILD) -M $@ "$(SOURCEDIR)" "$(BUILDDIR)" $(SPHINXOPTS) $(O) 
	rm -rf _build/edb_net
	mv _build/html _build/edb_net
	chmod -R 777 _build

epub: Makefile
	@$(SPHINXBUILD) -M $@ "$(SOURCEDIR)" "$(BUILDDIR)" $(SPHINXOPTS) $(O) 
	rm -rf _build/edb_net.epub
	cp _build/epub/EDBPostgresAdvancedServerNETDataProviderGuide.epub _build/edb_net.epub
	chmod -R 777 _build

latexpdf: Makefile
	@$(SPHINXBUILD) -M $@ "$(SOURCEDIR)" "$(BUILDDIR)" $(SPHINXOPTS) $(O) 
	rm -rf _build/edb_ocl.pdf
	cp _build/latex/NET.pdf _build/edb_net.pdf
	chmod -R 777 _build

