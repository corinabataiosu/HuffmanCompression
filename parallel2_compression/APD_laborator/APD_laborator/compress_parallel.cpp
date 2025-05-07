#include <mpi.h>
#include <iostream>
#include <fstream>
#include <vector>
#include <map>
#include <queue>
#include <sstream>
#include <cstring>
#include <algorithm>

using namespace std;

// huffman codes node
struct HuffmanNode {
    char ch;
    int freq;
    HuffmanNode* left;
    HuffmanNode* right;
    HuffmanNode(char c, int f) : ch(c), freq(f), left(nullptr), right(nullptr) {}
};

// comparing method for the priority queue
struct CompareNode {
    bool operator()(HuffmanNode* a, HuffmanNode* b) {
        return a->freq > b->freq;
    }
};

HuffmanNode* buildHuffmanTree(const map<char, int>& freqMap) {
    priority_queue<HuffmanNode*, vector<HuffmanNode*>, CompareNode> pq;
    for (auto& p : freqMap) {
        pq.push(new HuffmanNode(p.first, p.second));
    }
    while (pq.size() > 1) {
        HuffmanNode* left = pq.top(); pq.pop();
        HuffmanNode* right = pq.top(); pq.pop();
        HuffmanNode* parent = new HuffmanNode('\0', left->freq + right->freq);
        parent->left = left;
        parent->right = right;
        pq.push(parent);
    }
    return pq.top();
}

void generateHuffmanCodes(HuffmanNode* root, const string& code, map<char, string>& codes) {
    if (!root)
        return;
    if (!root->left && !root->right) {
        codes[root->ch] = code;
        return;
    }
    generateHuffmanCodes(root->left, code + "0", codes);
    generateHuffmanCodes(root->right, code + "1", codes);
}

void freeHuffmanTree(HuffmanNode* root) {
    if (!root)
        return;
    freeHuffmanTree(root->left);
    freeHuffmanTree(root->right);
    delete root;
}

// method fot rebuilding the huffman tree
HuffmanNode* buildTreeFromCodes(const map<char, string>& codes) {
    HuffmanNode* root = new HuffmanNode('\0', 0);
    for (auto& p : codes) {
        char ch = p.first;
        const string& code = p.second;
        HuffmanNode* current = root;
        for (char bit : code) {
            if (bit == '0') {
                if (!current->left)
                    current->left = new HuffmanNode('\0', 0);
                current = current->left;
            }
            else { // bit == '1'
                if (!current->right)
                    current->right = new HuffmanNode('\0', 0);
                current = current->right;
            }
        }
        // the character goes to the leaf
        current->ch = ch;
    }
    return root;
}

// decompress method
void decompressFile(const string& compressedFile, const string& outputFile) {
    ifstream inFile(compressedFile, ios::binary);
    if (!inFile) {
        cerr << "Error: Could not open compressed file for decompression!" << endl;
        return;
    }

    int headerLength = 0;
    inFile.read(reinterpret_cast<char*>(&headerLength), sizeof(int));
    if (!inFile) {
        cerr << "Error: Could not read header length!" << endl;
        return;
    }

    string header(headerLength, ' ');
    inFile.read(&header[0], headerLength);
    if (!inFile) {
        cerr << "Error: Could not read header!" << endl;
        return;
    }

    istringstream iss(header);
    long long originalFileSize = 0;
    iss >> originalFileSize;

    int mapSize = 0;
    iss >> mapSize;
    map<char, string> huffmanCodes;
    for (int i = 0; i < mapSize; i++) {
        int chInt;
        string code;
        iss >> chInt >> code;
        huffmanCodes[(char)chInt] = code;
    }

    // reconstruct the huffman nodes
    HuffmanNode* root = buildTreeFromCodes(huffmanCodes);

    // read the compressed 
    vector<unsigned char> compressedData((istreambuf_iterator<char>(inFile)),
        istreambuf_iterator<char>());
    inFile.close();

    // decompress by itereting through the bits and traversing the huffman tree
    ofstream outFile(outputFile, ios::binary);
    if (!outFile) {
        cerr << "Error: Could not open output file for decompression!" << endl;
        freeHuffmanTree(root);
        return;
    }

    HuffmanNode* current = root;
    long long decodedCount = 0;
    for (size_t i = 0; i < compressedData.size(); i++) {
        unsigned char byte = compressedData[i];
        for (int bit = 7; bit >= 0; bit--) {
            bool bitValue = byte & (1 << bit);
            if (bitValue)
                current = current->right;
            else
                current = current->left;

            // if the find the leaf, we write the characters
            if (!current->left && !current->right) {
                outFile.put(current->ch);
                decodedCount++;
                if (decodedCount == originalFileSize) {
                    outFile.close();
                    freeHuffmanTree(root);
                    return;
                }
                current = root;
            }
        }
    }
    outFile.close();
    freeHuffmanTree(root);
}

int main(int argc, char* argv[]) {
    MPI_Init(&argc, &argv);

    int rank, numProcs;
    MPI_Comm_rank(MPI_COMM_WORLD, &rank);
    MPI_Comm_size(MPI_COMM_WORLD, &numProcs);

    string inputFile = "125mbfile.txt";
    string outputBinaryFile = "output_compressed_file";

    double t_start1 = MPI_Wtime();

    long long fileSize = 0;
    if (rank == 0) {
        ifstream file(inputFile, ios::binary | ios::ate);
        if (!file) {
            cerr << "Error: Could not open input file!" << endl;
            MPI_Abort(MPI_COMM_WORLD, 1);
        }
        fileSize = file.tellg();
        file.close();
    }
    MPI_Bcast(&fileSize, 1, MPI_LONG_LONG, 0, MPI_COMM_WORLD);

    long long chunkSize = fileSize / numProcs;
    long long start = rank * chunkSize;
    long long end = (rank == numProcs - 1) ? fileSize : start + chunkSize;

    int localFreq[256] = { 0 };
    ifstream inFile(inputFile, ios::binary);
    if (!inFile) {
        cerr << "Error: Could not open input file on rank " << rank << "!" << endl;
        MPI_Abort(MPI_COMM_WORLD, 1);
    }
    inFile.seekg(start);
    long long toRead = end - start;
    const int bufferSize = 4096;
    char buffer[bufferSize];
    while (toRead > 0) {
        int readCount = min((long long)bufferSize, toRead);
        inFile.read(buffer, readCount);
        streamsize count = inFile.gcount();
        for (streamsize i = 0; i < count; i++) {
            unsigned char c = buffer[i];
            localFreq[c]++;
        }
        toRead -= count;
    }
    inFile.close();

    int globalFreq[256] = { 0 };
    MPI_Reduce(localFreq, globalFreq, 256, MPI_INT, MPI_SUM, 0, MPI_COMM_WORLD);

    map<char, int> freqMap;
    if (rank == 0) {
        for (int i = 0; i < 256; i++) {
            if (globalFreq[i] > 0)
                freqMap[(char)i] = globalFreq[i];
        }
    }

    double t_end1 = MPI_Wtime();
    if (rank == 0) {
        cout << "[DEBUG] Read file and compute frequencies: " << (t_end1 - t_start1) << " seconds\n";
    }

    // generating the huffman codes
    double t_start2 = MPI_Wtime();

    map<char, string> huffmanCodes;
    if (rank == 0) {
        HuffmanNode* root = buildHuffmanTree(freqMap);
        generateHuffmanCodes(root, "", huffmanCodes);
        freeHuffmanTree(root);
    }

    double t_end2 = MPI_Wtime();
    if (rank == 0) {
        cout << "[DEBUG] Generating the Huffman coding: " << (t_end2 - t_start2) << " seconds\n";
    }

    // we save the file size and huffman codes
    string serializedCodes;
    if (rank == 0) {
        ostringstream oss;
        // first line: original file size
        oss << fileSize << "\n";
        // second line: number of codes
        oss << huffmanCodes.size() << "\n";
        for (auto& p : huffmanCodes) {
            oss << static_cast<int>(p.first) << " " << p.second << "\n";
        }
        serializedCodes = oss.str();
    }

    int codesLength = serializedCodes.size();
    MPI_Bcast(&codesLength, 1, MPI_INT, 0, MPI_COMM_WORLD);
    if (rank != 0) {
        serializedCodes.resize(codesLength);
    }
    MPI_Bcast(&serializedCodes[0], codesLength, MPI_CHAR, 0, MPI_COMM_WORLD);

    if (rank != 0) {
        istringstream iss(serializedCodes);
        long long dummyFileSize;
        iss >> dummyFileSize; // read the original file size
        int mapSize;
        iss >> mapSize;
        for (int i = 0; i < mapSize; i++) {
            int ch;
            string code;
            iss >> ch >> code;
            huffmanCodes[(char)ch] = code;
        }
    }

    // compression
    double t_start3 = MPI_Wtime();

    vector<unsigned char> compressedData;
    unsigned char currentByte = 0;
    int bitCount = 0;

    ifstream compFile(inputFile, ios::binary);
    if (!compFile) {
        cerr << "Error: Could not re-open input file on rank " << rank << "!" << endl;
        MPI_Abort(MPI_COMM_WORLD, 1);
    }
    compFile.seekg(start);
    toRead = end - start;
    while (toRead > 0) {
        int readCount = min((long long)bufferSize, toRead);
        compFile.read(buffer, readCount);
        streamsize count = compFile.gcount();
        for (streamsize i = 0; i < count; i++) {
            char c = buffer[i];
            string code = huffmanCodes[c];
            for (char bit : code) {
                if (bit == '1')
                    currentByte |= (1 << (7 - bitCount));
                bitCount++;
                if (bitCount == 8) {
                    compressedData.push_back(currentByte);
                    currentByte = 0;
                    bitCount = 0;
                }
            }
        }
        toRead -= count;
    }
    compFile.close();
    if (bitCount > 0) {
        compressedData.push_back(currentByte);
    }

    double t_end3 = MPI_Wtime();
    if (rank == 0) {
        cout << "[DEBUG] Compression time: " << (t_end3 - t_start3) << " seconds\n";
    }

    int localCompressedSize = compressedData.size();
    vector<int> allSizes(numProcs);
    MPI_Allgather(&localCompressedSize, 1, MPI_INT, allSizes.data(), 1, MPI_INT, MPI_COMM_WORLD);

    vector<int> displacements(numProcs);
    int offset = 0;
    for (int i = 0; i < numProcs; i++) {
        displacements[i] = offset;
        offset += allSizes[i];
    }

    if (rank == 0) {
        ofstream outFile(outputBinaryFile, ios::binary);
        int headerLengthToWrite = serializedCodes.size();
        outFile.write(reinterpret_cast<char*>(&headerLengthToWrite), sizeof(int));
        outFile.write(serializedCodes.c_str(), headerLengthToWrite);
        outFile.close();
    }

    MPI_Barrier(MPI_COMM_WORLD);

    MPI_File fh;
    MPI_File_open(MPI_COMM_WORLD, outputBinaryFile.c_str(), MPI_MODE_WRONLY, MPI_INFO_NULL, &fh);

    MPI_Offset dataOffset = sizeof(int) + codesLength + displacements[rank];
    MPI_File_write_at(fh, dataOffset, compressedData.data(), localCompressedSize, MPI_UNSIGNED_CHAR, MPI_STATUS_IGNORE);

    MPI_File_close(&fh);

    if (rank == 0) {
        cout << "Compression complete. Total compressed size: " << offset << " bytes." << endl;
    }

    // decompression
    double t_start4 = MPI_Wtime();
    if (rank == 0) {
        decompressFile(outputBinaryFile, "decoded.txt");
        cout << "Decompression complete. " << endl;

        double t_end4 = MPI_Wtime();
        cout << "[DEBUG] Decompression time: " << (t_end4 - t_start4) << " seconds\n";
    }

    MPI_Finalize();

    return 0;
}