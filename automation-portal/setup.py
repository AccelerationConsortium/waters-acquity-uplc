"""
Setup script for Waters Acquity UPC Driver
"""

from setuptools import setup, find_packages

with open("README.md", "r", encoding="utf-8") as fh:
    long_description = fh.read()

with open("requirements.txt", "r", encoding="utf-8") as fh:
    requirements = [
        line.strip() for line in fh if line.strip() and not line.startswith("#")
    ]

setup(
    name="waters-acquity-driver",
    version="1.0.0",
    author="Kelvin Chow",
    author_email="kelvinchow23@users.noreply.github.com",
    description="Production-ready Python driver for Waters Acquity UPC systems "
    "with Automation Portal support",
    long_description=long_description,
    long_description_content_type="text/markdown",
    url="https://github.com/kelvinchow23/waters-acquity-upc",
    packages=find_packages(),
    classifiers=[
        "Development Status :: 5 - Production/Stable",
        "Intended Audience :: Science/Research",
        "License :: OSI Approved :: MIT License",
        "Operating System :: OS Independent",
        "Programming Language :: Python :: 3",
        "Programming Language :: Python :: 3.7",
        "Programming Language :: Python :: 3.8",
        "Programming Language :: Python :: 3.9",
        "Programming Language :: Python :: 3.10",
        "Programming Language :: Python :: 3.11",
        "Topic :: Scientific/Engineering",
        "Topic :: Scientific/Engineering :: Chemistry",
        "Topic :: Scientific/Engineering :: Interface Engine/Protocol Translator",
        "Topic :: System :: Hardware :: Hardware Drivers",
    ],
    keywords="waters acquity upc chromatography automation portal driver",
    python_requires=">=3.7",
    install_requires=requirements,
    extras_require={
        "dev": [
            "pytest>=6.0",
            "black>=21.0",
            "flake8>=3.8",
            "mypy>=0.910",
        ],
    },
    entry_points={
        "console_scripts": [
            "waters-acquity-demo=demo:main",
        ],
    },
)
