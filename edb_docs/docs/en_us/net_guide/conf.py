
# -- Project Details ----------------------------------

project = 'EDB .NET Connector'
version = '4.1.6.1'
release = '4.1.6.1'
copyright = '2020, EnterpriseDB'

# -- Project Variables --------------------------------

import os
import sys
sys.path.insert(0, os.path.abspath('.'))

# import the rst_epilog from variables.py file, rst_epilog contains the values of the variables
# from .variables import rst_epilog.  You can optionally use variable tags within the document to represent
# values:

rst_epilog = """
.. |variable_prod_version| replace:: X.Y
.. |variable_prod_name| replace:: Product Name
"""

# -- DO NOT CHANGE CONTENT AFTER THIS LINE ------------

# -- EDB Image Files ----------------------------------

# Image files reside in the same folder as the .rst files.
# Assorted favicons are available in the DOCUMENTS repo.

latex_logo = 'edb_logo.png'
html_logo = 'edb_logo.svg'
html_favicon = 'E.ico'

# -- General configuration ----------------------------

extensions = ['sphinx_tabs.tabs', 'sphinx_copybutton']
templates_path = ['_templates']
source_suffix = '.rst'
master_doc = 'index'
author = 'EnterpriseDB'
BUILDDIR= '_build'
exclude_patterns = ['_build', 'Thumbs.db', '.DS_Store']
pygments_style = 'sphinx'
todo_include_todos = False

# -- Naming Options for PDF output ---------------------

# PDF name and guide cover name; this name should match a value in the Makefile.  The latex_documents variable should specify 'manual' for guides, 'how-to' for Quick Starts.  These values should be set to match the existing values, but once set, they should not change:

latex_documents = [
    (master_doc, 'edb_net.tex', 'EDB .NET Connector',
     'EDB .NET Connector User\'s Guide', 'manual'),
]

# -- Naming Options for EPUB output --------------------

# The following statement names the EPUB file.  Once set, it should not change (the value is used in the Makefile):

epub_basename = 'edb_net'

# -- Options for HTML output ---------------------------

html_show_sphinx = False
html_show_copyright = True
html_theme = 'edb'
html_theme_path = ['theme']
html_static_path = ['_static']
html_sidebars = {
    '**': [
        'relations.html',  # needs 'show_related': True theme option to display
        'searchbox.html',
        'globaltoc.html'
    ]
}

# -- Build Options for LaTeX output --------------------
latex_elements = {
'papersize': 'letter',
'pointsize': '12pt',
 'figure_align': 'H',
'extraclassoptions': 'openany,oneside',
'fncychap' : r'\usepackage[Sonny]{fncychap}',
'preamble': r'''
  \usepackage{hyperref}
  \usepackage{cmap}
  \setcounter{tocdepth}{3}
'''
}
