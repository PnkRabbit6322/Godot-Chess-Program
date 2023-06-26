using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using static System.String;

public class Board : TileMap
{
    public enum enumSquare {
        a1, b1, c1, d1, e1, f1, g1, h1,
        a2, b2, c2, d2, e2, f2, g2, h2,
        a3, b3, c3, d3, e3, f3, g3, h3,
        a4, b4, c4, d4, e4, f4, g4, h4,
        a5, b5, c5, d5, e5, f5, g5, h5,
        a6, b6, c6, d6, e6, f6, g6, h6,
        a7, b7, c7, d7, e7, f7, g7, h7,
        a8, b8, c8, d8, e8, f8, g8, h8
    }

    public string[] square = {
        "a1", "b1", "c1", "d1", "e1", "f1", "g1", "h1",
        "a2", "b2", "c2", "d2", "e2", "f2", "g2", "h2",
        "a3", "b3", "c3", "d3", "e3", "f3", "g3", "h3",
        "a4", "b4", "c4", "d4", "e4", "f4", "g4", "h4",
        "a5", "b5", "c5", "d5", "e5", "f5", "g5", "h5",
        "a6", "b6", "c6", "d6", "e6", "f6", "g6", "h6",
        "a7", "b7", "c7", "d7", "e7", "f7", "g7", "h7",
        "a8", "b8", "c8", "d8", "e8", "f8", "g8", "h8",
    };

    enum enumPiece {
        P, N, B, R, Q, K, p, n, b, r, q, k
    }

    public enum enumSide {
        white, black
    }

    enum enumCastle 
    {
        wk = 1, wq = 2, bk = 4, bq = 8,
    }

    public enum enumRookBishop {
        rook, bishop
    }

    // bitboard shift direction
    // noWe = 7, nort = 8, noEa = 9,
    // west = -1, mid = 0, east = 1,
    // soWe = -9, sout = -8, soEa = -7

    int[] bishop_relevant_bits ={
    6, 5, 5, 5, 5, 5, 5, 6, 
    5, 5, 5, 5, 5, 5, 5, 5, 
    5, 5, 7, 7, 7, 7, 5, 5, 
    5, 5, 7, 9, 9, 7, 5, 5, 
    5, 5, 7, 9, 9, 7, 5, 5, 
    5, 5, 7, 7, 7, 7, 5, 5, 
    5, 5, 5, 5, 5, 5, 5, 5, 
    6, 5, 5, 5, 5, 5, 5, 6
    };

    int[] rook_relevant_bits = {
    12, 11, 11, 11, 11, 11, 11, 12, 
    11, 10, 10, 10, 10, 10, 10, 11, 
    11, 10, 10, 10, 10, 10, 10, 11, 
    11, 10, 10, 10, 10, 10, 10, 11, 
    11, 10, 10, 10, 10, 10, 10, 11, 
    11, 10, 10, 10, 10, 10, 10, 11, 
    11, 10, 10, 10, 10, 10, 10, 11, 
    12, 11, 11, 11, 11, 11, 11, 12
    };
    public void print_bitboard(UInt64 bb)
    {
        var output = new Godot.Collections.Array();
        for (int rank = 0; rank < 8; rank++)
        {
            for (int file = 0; file < 8; file++)
            {
                int square = rank * 8 + file;

                if(file == 0) output.Add(rank + 1);

                output.Add((get_bit(bb, square) != 0) ? 1 : 0);
            }
            GD.Print(output.ToString());
            output.Clear();
            GD.Print(" ");
        }
        output.Add("0");
        output.Add("a");
        output.Add("b");
        output.Add("c");
        output.Add("d");
        output.Add("e");
        output.Add("f");
        output.Add("g");
        output.Add("h");
        GD.Print(output.ToString());
        GD.Print("bitboard ", bb);
    }

    public UInt64 set_bit(UInt64 bb, int pos)
    {
        return bb | ((UInt64)1 << pos);
    }

    public UInt64 get_bit(UInt64 bb, int pos)
    {
        return bb & ((UInt64)1 << pos);
    }

    public int count_bits(UInt64 bitboard)
    {
        int count = 0;

        while(!bitboard.Equals(empty))
        {
            count++;
            bitboard &= bitboard - 1;
        }

        return count;
    }

    UInt64 random_uint64()
    {
        Random rnd = new Random();
        UInt64 u1, u2, u3, u4;
        u1 = (UInt64)rnd.Next(0, (int)0x7fff) & 0xffff;
        u2 = (UInt64)rnd.Next(0, (int)0x7fff) & 0xffff;
        u3 = (UInt64)rnd.Next(0, (int)0x7fff) & 0xffff;
        u4 = (UInt64)rnd.Next(0, (int)0x7fff) & 0xffff;

        return u1 | (u2 << 16) | (u3 << 32) | (u4 << 48);
    }

    UInt64 random_uint64_fewbits()
    {
        return random_uint64() & random_uint64() & random_uint64();
    }
    int[] index64 = {
    0,  1, 48,  2, 57, 49, 28,  3,
    61, 58, 50, 42, 38, 29, 17,  4,
    62, 55, 59, 36, 53, 51, 43, 22,
    45, 39, 33, 30, 24, 18, 12,  5,
    63, 47, 56, 27, 60, 41, 37, 16,
    54, 35, 52, 21, 44, 32, 23, 11,
    46, 26, 40, 15, 34, 20, 31, 10,
    25, 14, 19,  9, 13,  8,  7,  6
    };

    public int ls1b_index(UInt64 bb)
    {
        const UInt64 debruijn64 = 0x03f79d71b4cb0a89; 
        return index64[((bb & (~bb + 1)) * debruijn64) >> 58];
    }

    public int[] get_bits_index_array(UInt64 bb)
    {
        int count = count_bits(bb);
        int[] index_array = new int[count];

        for (int i = 0; i < count; i++)
        {
            index_array[i] = ls1b_index(bb);
            bb &= bb - 1;
        }

        return index_array; 
    }

    public Vector2 square_to_pos(int square)
    {
        Vector2 pos = new Vector2(0, 0);

        for (pos.y = 0; pos.y < 8; pos.y++)
        {
            pos.x = 8 * pos.y - (56 - square);
            if (pos.x >= 0 && pos.x <= 7) return pos;
        }

        return pos;
    }

    public int pos_to_square(Vector2 pos)
    {
        return (int)((7 - pos.y) * 8 + pos.x);
    }

    // private readonly UInt64[] file = {
    //     0x0101010101010101, 0x0202020202020202, 0x0404040404040404, 0x0808080808080808,
    //     0x1010101010101010, 0x2020202020202020, 0x4040404040404040, 0x8080808080808080
    // };

    private const UInt64 a_file = 0x0101010101010101;
    private const UInt64 rank1 = 0x00000000000000ff;
    private const UInt64 a1_h8_diag = 0x8040201008040201;
    private const UInt64 h1_a8_diag = 0x0102040810204080;
    private const UInt64 not_a_file = 0xfefefefefefefefe; // ~0x0101010101010101
    private const UInt64 not_ab_file = 0xFCFCFCFCFCFCFCFC;
    private const UInt64 not_hg_file = 0x3F3F3F3F3F3F3F3F;
    private const UInt64 not_h_file = 0x7f7f7f7f7f7f7f7f; // ~0x8080808080808080
    private const UInt64 empty = 0x0000000000000000;
    
    public UInt64 soutOne(UInt64 b) { return b >> 8;}
    public UInt64 nortOne(UInt64 b) { return b << 8;}
    public UInt64 eastOne(UInt64 b) { return (b << 1) & not_a_file;}
    public UInt64 noEaOne(UInt64 b) { return (b << 9) & not_a_file;}
    public UInt64 soEaOne(UInt64 b) { return (b >> 7) & not_a_file;}
    public UInt64 westOne(UInt64 b) { return (b >> 1) & not_h_file;}
    public UInt64 noWeOne(UInt64 b) { return (b << 7) & not_h_file;}
    public UInt64 soWeOne(UInt64 b) { return (b >> 9) & not_h_file;}

    UInt64[] knight_attack = new UInt64[64];
    UInt64[,] pawn_attack = new UInt64[2,64];
    UInt64[] king_attack = new UInt64[64];
    UInt64[] bishop_attack = new UInt64[64];
    UInt64[] rook_attack = new UInt64[64];
    public UInt64 mask_knight_attack(int square)
    {
        UInt64 attacks_bb = 0;
        UInt64 pieces_bb = set_bit((UInt64)0, square);

        if (!(((pieces_bb >> 17) & not_h_file).Equals(empty))) attacks_bb = attacks_bb | (pieces_bb >> 17);  
        if (!(((pieces_bb >> 15) & not_a_file).Equals(empty))) attacks_bb = attacks_bb | (pieces_bb >> 15);

        if (!(((pieces_bb << 17) & not_a_file).Equals(empty))) attacks_bb = attacks_bb | (pieces_bb << 17);
        if (!(((pieces_bb << 15) & not_h_file).Equals(empty))) attacks_bb = attacks_bb | (pieces_bb << 15);

        if (!(((pieces_bb >> 10) & not_hg_file).Equals(empty))) attacks_bb = attacks_bb | (pieces_bb >> 10);
        if (!(((pieces_bb << 10) & not_ab_file).Equals(empty))) attacks_bb = attacks_bb | (pieces_bb << 10);

        if (!(((pieces_bb >> 6) & not_ab_file).Equals(empty))) attacks_bb = attacks_bb | (pieces_bb >> 6);
        if (!(((pieces_bb << 6) & not_hg_file).Equals(empty))) attacks_bb = attacks_bb | (pieces_bb << 6);
        return attacks_bb;
    }

    public UInt64 mask_pawn_attack(int side, int square)
    {
        UInt64 attacks_bb = 0;
        UInt64 pieces_bb = set_bit((UInt64)0, square);

        if (side == 1) {
            if (!(soEaOne(pieces_bb).Equals(empty))) attacks_bb = attacks_bb | soEaOne(pieces_bb);
            if (!(soWeOne(pieces_bb).Equals(empty))) attacks_bb = attacks_bb | soWeOne(pieces_bb);
        } else {
            if (!(noWeOne(pieces_bb).Equals(empty))) attacks_bb = attacks_bb | noWeOne(pieces_bb);
            if (!(noEaOne(pieces_bb).Equals(empty))) attacks_bb = attacks_bb | noEaOne(pieces_bb);
        }
        return attacks_bb;
    }

    public UInt64 mask_king_attack(int square)
    {
        UInt64 attacks_bb = 0;
        UInt64 pieces_bb = set_bit((UInt64)0, square);

        attacks_bb = attacks_bb | nortOne(pieces_bb);
        attacks_bb = attacks_bb | soutOne(pieces_bb);
        if (!(westOne(pieces_bb).Equals(empty))) attacks_bb = attacks_bb | westOne(pieces_bb);
        if (!(soWeOne(pieces_bb).Equals(empty))) attacks_bb = attacks_bb | soWeOne(pieces_bb);
        if (!(noWeOne(pieces_bb).Equals(empty))) attacks_bb = attacks_bb | noWeOne(pieces_bb);
        if (!(noEaOne(pieces_bb).Equals(empty))) attacks_bb = attacks_bb | noEaOne(pieces_bb);
        if (!(soEaOne(pieces_bb).Equals(empty))) attacks_bb = attacks_bb | soEaOne(pieces_bb);
        if (!(eastOne(pieces_bb).Equals(empty))) attacks_bb = attacks_bb | eastOne(pieces_bb);

        return attacks_bb;
    }

    public UInt64 set_occupancy(int index, int bits_in_mask, UInt64 attack_mask)
    {
        int i, square;
        UInt64 occupancy_map = 0;

        for(i = 0; i < bits_in_mask; i++)
        {
            square = ls1b_index(attack_mask);

            attack_mask = attack_mask & (attack_mask - 1);

            if ((index & (1 << i)) != 0)
            {
                occupancy_map = occupancy_map | ((UInt64)1 << square);
            }
        }

        return occupancy_map;
    }
    public UInt64 mask_bishop_attack(int square)
    {
        UInt64 attacks_bb = 0;

        //Rank & File
        int r, f;

        //Target Rank & File
        int tr = square / 8;
        int tf = square % 8;

        for(r = tr + 1, f = tf + 1; r <= 6 && f <= 6; r++, f++) attacks_bb = attacks_bb | ((UInt64)1 << (r * 8 + f));
        for(r = tr - 1, f = tf + 1; r >= 1 && f <= 6; r--, f++) attacks_bb = attacks_bb | ((UInt64)1 << (r * 8 + f));
        for(r = tr + 1, f = tf - 1; r <= 6 && f >= 1; r++, f--) attacks_bb = attacks_bb | ((UInt64)1 << (r * 8 + f));
        for(r = tr - 1, f = tf - 1; r >= 1 && f >= 1; r--, f--) attacks_bb = attacks_bb | ((UInt64)1 << (r * 8 + f));

        return attacks_bb;
    }

    public UInt64 bishop_attack_with_block(int square, UInt64 block)
    {
        UInt64 attacks_bb = 0;

        //Rank & File
        int r, f;

        //Target Rank & File
        int tr = square / 8;
        int tf = square % 8;

        for(r = tr + 1, f = tf + 1; r <= 7 && f <= 7; r++, f++)
        {
            attacks_bb = attacks_bb | ((UInt64)1 << (r * 8 + f));
            if (!(((UInt64)1 << r * 8 + f) & block).Equals(empty)) break;
        }
        for(r = tr - 1, f = tf + 1; r >= 0 && f <= 7; r--, f++)
        {
            attacks_bb = attacks_bb | ((UInt64)1 << (r * 8 + f));
            if (!(((UInt64)1 << r * 8 + f) & block).Equals(empty)) break;
        }
        for(r = tr + 1, f = tf - 1; r <= 7 && f >= 0; r++, f--)
        {
            attacks_bb = attacks_bb | ((UInt64)1 << (r * 8 + f));
            if (!(((UInt64)1 << r * 8 + f) & block).Equals(empty)) break;
        }
        for(r = tr - 1, f = tf - 1; r >= 0 && f >= 0; r--, f--)
        {
            attacks_bb = attacks_bb | ((UInt64)1 << (r * 8 + f));
            if (!(((UInt64)1 << r * 8 + f) & block).Equals(empty)) break;
        }

        return attacks_bb;
    }

    public UInt64 mask_rook_attack(int square)
    {
        UInt64 attacks_bb = 0;

        //Rank & File
        int r, f;

        //Target Rank & File
        int tr = square / 8;
        int tf = square % 8;

        for(r = tr + 1; r <= 6; r++) attacks_bb = attacks_bb | ((UInt64)1 << (r * 8 + tf));
        for(r = tr - 1; r >= 1; r--) attacks_bb = attacks_bb | ((UInt64)1 << (r * 8 + tf));
        for(f = tf + 1; f <= 6; f++) attacks_bb = attacks_bb | ((UInt64)1 << (tr * 8 + f));
        for(f = tf - 1; f >= 1; f--) attacks_bb = attacks_bb | ((UInt64)1 << (tr * 8 + f));

        return attacks_bb;
    }

    public UInt64 rook_attack_with_block(int square, UInt64 block)
    {
        UInt64 attacks_bb = 0;

        //Rank & File
        int r, f;

        //Target Rank & File
        int tr = square / 8;
        int tf = square % 8;

        for(r = tr + 1; r <= 7; r++)
        {
            attacks_bb = attacks_bb | ((UInt64)1 << (r * 8 + tf));
            if(!((((UInt64)1 << (r * 8 + tf)) & block).Equals(empty))) break;
        }
        for(r = tr - 1; r >= 0; r--)
        {
            attacks_bb = attacks_bb | ((UInt64)1 << (r * 8 + tf));
            if(!((((UInt64)1 << (r * 8 + tf)) & block).Equals(empty))) break;
        }
        for(f = tf + 1; f <= 7; f++)
        {
            attacks_bb = attacks_bb | ((UInt64)1 << (tr * 8 + f));
            if(!((((UInt64)1 << (tr * 8 + f)) & block).Equals(empty))) break;
        }
        for(f = tf - 1; f >= 0; f--)
        {
            attacks_bb = attacks_bb | ((UInt64)1 << (tr * 8 + f));
            if(!((((UInt64)1 << (tr * 8 + f)) & block).Equals(empty))) break;
        }

        return attacks_bb;
    }

    public void init_magic_numbers()
    {
        for (int square = 0; square < 64; square++)
        {
            rook_magic_number[square] = find_magic_number(square, rook_relevant_bits[square], enumRookBishop.rook);
        }

        for (int square = 0; square < 64; square++)
        {
            bishop_magic_number[square] = find_magic_number(square, bishop_relevant_bits[square], enumRookBishop.bishop);
        }
    }

    UInt64 find_magic_number(int square, int relevant_bits, enumRookBishop bishop)
    {
        UInt64 magic;
        UInt64[] occupancies_arr = new UInt64[4096];
        UInt64[] attacks_table = new UInt64[4096];
        UInt64[] used_attack = new UInt64[4096];

        UInt64 attack_mask = (bishop == enumRookBishop.bishop) ? mask_bishop_attack(square) : mask_rook_attack(square);
        int n = count_bits(attack_mask);

        for (int i = 0; i < (1 << n); i++)
        {
            occupancies_arr[i] = set_occupancy(i, n, attack_mask);
            attacks_table[i] = (bishop == enumRookBishop.bishop) ? bishop_attack_with_block(square, occupancies_arr[i]) : rook_attack_with_block(square, occupancies_arr[i]);
        }

        for (int j = 0; j < 100000000; j++)
        {
            magic = random_uint64_fewbits();

            if (count_bits((attack_mask * magic) & 0xFF00000000000000) < 6) continue;

            for (int i = 0; i < 4096; i++) used_attack[i] = (UInt64)0;

            int index, fail;
            for (index = 0, fail = 0; fail == 0 && index < (1 << n); index++)
            {
                int magic_index = (int)((occupancies_arr[index] * magic) >> (64 - relevant_bits));
                if(used_attack[magic_index] == (UInt64)0)
                    used_attack[magic_index] = attacks_table[index];
                else if (used_attack[magic_index] != attacks_table[index])
                    fail = 1;
            }

            if (fail == 0)
                return magic;
        }
        return (UInt64)0;
    }

    UInt64[] rook_magic_number = new UInt64[64];
    UInt64[] bishop_magic_number = new UInt64[64];

    UInt64[] pre_cal_rook_magic_number = {
        0x0080028160104001, 0x0440042000100040, 0x0200220010800840, 0x01800C0800811000,
        0x0480020800810400, 0x0500440002010008, 0x2180008002000100, 0x0600050408802042,
        0x2001002040800100, 0x0000400020005004, 0x100100110C200040, 0x00C2002010084201,
        0x2805001008000500, 0x0402000200040810, 0x1809000402000100, 0x00C6000040810412,
        0x4A00218000804016, 0x0050004000402001, 0x1001010020004018, 0x02AA020020081040,
        0x2148010004100900, 0x0001010002040008, 0x4800040010020801, 0x0018020004004081,
        0x0080005040002002, 0x2010400040201000, 0x405C110100402000, 0x10010009001001A0,
        0x1228080100041100, 0x0004004040020100, 0x0040020400014850, 0x0000040200006081,
        0x0200400020800080, 0x0038201000400040, 0x021000A003801282, 0x0090082301001002,
        0x4402000412000820, 0x00C4008004804200, 0x6021000401000200, 0x0401002841000082,
        0x0C80002000404000, 0x0010004020014000, 0x0000110020010044, 0x0004100100210008,
        0x0022002050060008, 0x000A010408020010, 0x2208011208140050, 0x1002248400420011,
        0x00A0400024800280, 0x01101120004000C0, 0x0A05001020004900, 0x0201082242011200,
        0x0024008008020480, 0x2406008034003280, 0x000D000402000100, 0x0800140042970200,
        0x0004104602802102, 0x0210204011020486, 0x0000100A20010241, 0x4000041000200901,
        0x5821000800021005, 0x001100084400120B, 0x0000480112100084, 0x0200110040862402
    };

    UInt64[] pre_cal_bishop_magic_number = {
        0x4120091002224a40, 0x02600a0206490000, 0x1041012200800000, 0x0038184500000208,
        0x0801104100102801, 0x2046480240200080, 0x00010108600412a2, 0x1018420050021010,
        0x4204102051410201, 0x038a104102040040, 0x0000222805002000, 0x12001c040c844000,
        0x0a00040420000105, 0x0800124150400400, 0x0a4044010802100a, 0x0810003c01080800,
        0x2209101002104410, 0x000840900138008b, 0x003800d000204090, 0x4010225208220000,
        0x0021000820080040, 0x0002004020842000, 0x0196000108094400, 0x1042202943041000,
        0x2004608a04200408, 0x000e0a8810100242, 0x008c280004004402, 0x01c00400004100a0,
        0x4001010000104000, 0x0009010006004150, 0x0004005014010450, 0x0004008404220100,
        0x110808081a046000, 0x000a080220215201, 0x0001034808050802, 0x0802200801110104,
        0x0020420020420081, 0x4044010208041000, 0x0402083110804420, 0x5448008100602120,
        0x0048441008000408, 0x0124141c04840208, 0x0021001802000400, 0x0100120204250a00,
        0x0206441009800408, 0x01c0040400220c10, 0x0002440822204880, 0x1042080864801101,
        0x2001040260042000, 0x0002170108021000, 0x2010202201100090, 0x0040482108680000,
        0x2120010a10240441, 0x0000040408321006, 0x0004200204210140, 0x0028080800813430,
        0x0600404044202004, 0x0000008431080204, 0x000010004a180420, 0x1000002a04840400,
        0x1412000230120880, 0x0000081120410108, 0x00840448060c4410, 0x005010d000802040,
    };

    public void init_leaper_attack()
    {
        for(int i = 0; i < 64; i++)
        {
            pawn_attack[(int)enumSide.white, i] = mask_pawn_attack((int)enumSide.white, i);
            pawn_attack[(int)enumSide.black, i] = mask_pawn_attack((int)enumSide.black, i);
            knight_attack[i] = mask_knight_attack(i);
            king_attack[i] = mask_king_attack(i);
        }
    }

    UInt64[] bishop_masks = new UInt64[64];
    UInt64[] rook_masks = new UInt64[64];
    UInt64[,] bishop_attacks = new UInt64[64, 512];
    UInt64[,] rook_attacks = new UInt64[64, 4096];

    public void init_slider_attacks(enumRookBishop bishop)
    {
        for (int square = 0; square < 64; square++)
        {
            bishop_masks[square] = mask_bishop_attack(square);
            rook_masks[square] = mask_rook_attack(square);

            UInt64 attack_mask = (bishop == enumRookBishop.bishop) ? bishop_masks[square] : rook_masks[square];

            int relevant_bits_count = count_bits(attack_mask);
            for(int i = 0; i < (1 << relevant_bits_count); i++)
            {
                if(bishop == enumRookBishop.bishop)
                {
                    UInt64 occupancy = set_occupancy(i, relevant_bits_count, attack_mask);
                    int magic_index = (int)((occupancy * pre_cal_bishop_magic_number[square]) >> (64 - bishop_relevant_bits[square]));
                    bishop_attacks[square,magic_index] = bishop_attack_with_block(square, occupancy);
                } else {
                    UInt64 occupancy = set_occupancy(i, relevant_bits_count, attack_mask);
                    int magic_index = (int)((occupancy * pre_cal_rook_magic_number[square]) >> (64 - rook_relevant_bits[square]));
                    rook_attacks[square,magic_index] = rook_attack_with_block(square, occupancy);
                }
            }
        }
    }

    public UInt64 get_bishop_attacks(int square, UInt64 block)
    {
        block = block & bishop_masks[square];
        block = block * pre_cal_bishop_magic_number[square];
        block = block >> (64 - bishop_relevant_bits[square]);

        return bishop_attacks[square,block];
    }

    public UInt64 get_rook_attacks(int square, UInt64 block)
    {
        block = block & rook_masks[square];
        block = block * pre_cal_rook_magic_number[square];
        block = block >> (64 - rook_relevant_bits[square]);

        return rook_attacks[square,block];
    }

    public UInt64 get_queen_attacks(int square, UInt64 block)
    {
        UInt64 queen_attacks = (UInt64)0;

        UInt64 bishop_block = block;
        UInt64 rook_block = block;

        bishop_block = bishop_block & bishop_masks[square];
        bishop_block = bishop_block * pre_cal_bishop_magic_number[square];
        bishop_block = bishop_block >> (64 - bishop_relevant_bits[square]);

        queen_attacks = bishop_attacks[square, bishop_block];

        rook_block = rook_block & rook_masks[square];
        rook_block = rook_block * pre_cal_rook_magic_number[square];
        rook_block = rook_block >> (64 - rook_relevant_bits[square]);

        queen_attacks = queen_attacks | rook_attacks[square, rook_block];

        return queen_attacks;
    }

    public void init_all()
    {
        // init_magic_numbers(); //Pre-Cal complete
        init_leaper_attack();
        init_slider_attacks(enumRookBishop.bishop);
        init_slider_attacks(enumRookBishop.rook);
    }

    UInt64 wPiece_bb, bPiece_bb;
    UInt64[] pieces_bb = new UInt64[12];

    public int get_piece_type(int square)
    {
        UInt64 bb = (UInt64)1 << square;
        for (int i = 0; i < 12; i++)
        {
            if (!(pieces_bb[i] & bb).Equals(empty)) return i;
        }
        return -1;
    }

    public void gen_bitboard()
    {
        //generate 14 bitboard: white piece, black piece, knight(w/b), rook(w/b), bishop(w/b), queen(w/b), king(w/b), pawn(w/b)
        var white_pieces = new Godot.Collections.Array();
        var black_pieces = new Godot.Collections.Array();
        var knights = new Godot.Collections.Array();
        var rooks = new Godot.Collections.Array();
        var bishops = new Godot.Collections.Array();
        var queens = new Godot.Collections.Array();
        var kings = new Godot.Collections.Array();
        var pawns = new Godot.Collections.Array();

        white_pieces = GetTree().GetNodesInGroup("white_pieces");
        black_pieces = GetTree().GetNodesInGroup("black_pieces");
        knights = GetTree().GetNodesInGroup("knights");
        rooks = GetTree().GetNodesInGroup("rooks");
        bishops = GetTree().GetNodesInGroup("bishops");
        queens = GetTree().GetNodesInGroup("queens");
        kings = GetTree().GetNodesInGroup("kings");
        pawns = GetTree().GetNodesInGroup("pawns");

        wPiece_bb = bPiece_bb = empty;
        for (int i = 0; i < 12; i++)
        {
            pieces_bb[i] = empty;
        }

        castle_right = 15; //binary 1111: both side can castle king & queen side
        enpassant_target = -1;
        enpassant_bitboard = 0;

        Vector2 pos;
        UInt64 bb = empty;

        foreach (Node2D piece in white_pieces)
        {
            pos = this.WorldToMap(piece.GlobalPosition);
            wPiece_bb = wPiece_bb | set_bit(wPiece_bb, (int)((7 - pos.y) * 8 + pos.x));
        }

        foreach (Node2D piece in black_pieces)
        {
            pos = this.WorldToMap(piece.GlobalPosition);
            bPiece_bb = bPiece_bb | set_bit(bPiece_bb, (int)((7 - pos.y) * 8 + pos.x));
        }

        foreach (Node2D piece in knights)
        {
            pos = this.WorldToMap(piece.GlobalPosition);
            bb = bb | set_bit(bb, (int)((7 - pos.y) * 8 + pos.x));
        }
        pieces_bb[(int)enumPiece.N] = bb & wPiece_bb;
        pieces_bb[(int)enumPiece.n] = bb & bPiece_bb;
        bb = empty;

        foreach (Node2D piece in rooks)
        {
            pos = this.WorldToMap(piece.GlobalPosition);
            bb = bb | set_bit(bb, (int)((7 - pos.y) * 8 + pos.x));
        }
        pieces_bb[(int)enumPiece.R] = bb & wPiece_bb;
        pieces_bb[(int)enumPiece.r] = bb & bPiece_bb;
        bb = empty;

        foreach (Node2D piece in bishops)
        {
            pos = this.WorldToMap(piece.GlobalPosition);
            bb = bb | set_bit(bb, (int)((7 - pos.y) * 8 + pos.x));
        }
        pieces_bb[(int)enumPiece.B] = bb & wPiece_bb;
        pieces_bb[(int)enumPiece.b] = bb & bPiece_bb;
        bb = empty;

        foreach (Node2D piece in queens)
        {
            pos = this.WorldToMap(piece.GlobalPosition);
            bb = bb | set_bit(bb, (int)((7 - pos.y) * 8 + pos.x));
        }
        pieces_bb[(int)enumPiece.Q] = bb & wPiece_bb;
        pieces_bb[(int)enumPiece.q] = bb & bPiece_bb;
        bb = empty;

        foreach (Node2D piece in kings)
        {
            pos = this.WorldToMap(piece.GlobalPosition);
            bb = bb | set_bit(bb, (int)((7 - pos.y) * 8 + pos.x));
        }
        pieces_bb[(int)enumPiece.K] = bb & wPiece_bb;
        pieces_bb[(int)enumPiece.k] = bb & bPiece_bb;
        bb = empty;

        foreach (Node2D piece in pawns)
        {
            pos = this.WorldToMap(piece.GlobalPosition);
            bb = bb | set_bit(bb, (int)((7 - pos.y) * 8 + pos.x));
        }
        pieces_bb[(int)enumPiece.P] = bb & wPiece_bb;
        pieces_bb[(int)enumPiece.p] = bb & bPiece_bb;
        bb = empty;
    }

    //check if a square is attacked by any piece of side with current board state, return attacker bitboard
    public UInt64 is_square_attacked(int square, enumSide side)
    {
        UInt64 attacker_mask = 0;

        if ((side == enumSide.white) && !((pawn_attack[(int)enumSide.black, square] & pieces_bb[(int)enumPiece.P]).Equals(empty))) 
            attacker_mask |= pawn_attack[(int)enumSide.black, square] & pieces_bb[(int)enumPiece.P];
        if ((side == enumSide.black) && !((pawn_attack[(int)enumSide.white, square] & pieces_bb[(int)enumPiece.p]).Equals(empty)))
            attacker_mask |= pawn_attack[(int)enumSide.white, square] & pieces_bb[(int)enumPiece.p];

        if (!((knight_attack[square] & ((side == enumSide.white) ? pieces_bb[(int)enumPiece.N] : pieces_bb[(int)enumPiece.n])).Equals(empty)))
            attacker_mask |= knight_attack[square] & ((side == enumSide.white) ? pieces_bb[(int)enumPiece.N] : pieces_bb[(int)enumPiece.n]);
        if (!((king_attack[square] & ((side == enumSide.white) ? pieces_bb[(int)enumPiece.K] : pieces_bb[(int)enumPiece.k])).Equals(empty)))
            attacker_mask |= king_attack[square] & ((side == enumSide.white) ? pieces_bb[(int)enumPiece.K] : pieces_bb[(int)enumPiece.k]);

        if (!((get_rook_attacks(square, (wPiece_bb | bPiece_bb)) & ((side == enumSide.white) ? pieces_bb[(int)enumPiece.R] : pieces_bb[(int)enumPiece.r])).Equals(empty)))
            attacker_mask |= get_rook_attacks(square, (wPiece_bb | bPiece_bb)) & ((side == enumSide.white) ? pieces_bb[(int)enumPiece.R] : pieces_bb[(int)enumPiece.r]);
        if (!((get_bishop_attacks(square, (wPiece_bb | bPiece_bb)) & ((side == enumSide.white) ? pieces_bb[(int)enumPiece.B] : pieces_bb[(int)enumPiece.b])).Equals(empty)))
            attacker_mask |= get_bishop_attacks(square, (wPiece_bb | bPiece_bb)) & ((side == enumSide.white) ? pieces_bb[(int)enumPiece.B] : pieces_bb[(int)enumPiece.b]);
        if (!((get_queen_attacks(square, (wPiece_bb | bPiece_bb)) & ((side == enumSide.white) ? pieces_bb[(int)enumPiece.Q] : pieces_bb[(int)enumPiece.q])).Equals(empty))) 
            attacker_mask |= get_queen_attacks(square, (wPiece_bb | bPiece_bb)) & ((side == enumSide.white) ? pieces_bb[(int)enumPiece.Q] : pieces_bb[(int)enumPiece.q]);

        return attacker_mask;
    }

    // return checkers bitboard
    public UInt64 is_in_check(enumSide side)
    {
        //is white in check
        if (side == enumSide.white)
            return is_square_attacked(ls1b_index(pieces_bb[(int)enumPiece.K]), enumSide.black);
        //is black in check
        if (side == enumSide.black)
            return is_square_attacked(ls1b_index(pieces_bb[(int)enumPiece.k]), enumSide.white);

        return (UInt64)0;
    }

    //get target square for all pawn push
    public UInt64 wPawnsSinglePushTarget(UInt64 wPawn, UInt64 emptySquare)
    {
        return nortOne(wPawn) & emptySquare;
    }
    public UInt64 bPawnsSinglePushTarget(UInt64 bPawn, UInt64 emptySquare)
    {
        return soutOne(bPawn) & emptySquare;
    }
    public UInt64 wPawnsDoublePushTarget(UInt64 wPawn, UInt64 emptySquare)
    {
        UInt64 rank4 = (UInt64)0x00000000FF000000;
        UInt64 singlePushs = wPawnsSinglePushTarget(wPawn, emptySquare);
        return nortOne(singlePushs) & emptySquare & rank4;
    }
    public UInt64 bPawnsDoublePushTarget(UInt64 bPawn, UInt64 emptySquare)
    {
        UInt64 rank5 = (UInt64)0x000000FF00000000;
        UInt64 singlePushs = bPawnsSinglePushTarget(bPawn, emptySquare);
        return soutOne(singlePushs) & emptySquare & rank5;
    }

    //get source square for all pawn push
    public UInt64 wPawnsSinglePushSource(UInt64 wPawn, UInt64 emptySquare)
    {
        return soutOne(emptySquare) & wPawn;
    }
    public UInt64 wPawnsDoublePushSource(UInt64 wPawn, UInt64 emptySquare)
    {
        UInt64 rank4 = 0x00000000FF000000;
        UInt64 emptySquareRank3 = soutOne(emptySquare & rank4) & emptySquare;
        return wPawnsSinglePushSource(wPawn ,emptySquareRank3);
    }
    public UInt64 bPawnsSinglePushSource(UInt64 bPawn, UInt64 emptySquare)
    {
        return nortOne(emptySquare) & bPawn;
    }
    public UInt64 bPawnsDoublePushSource(UInt64 bPawn, UInt64 emptySquare)
    {
        UInt64 rank5 = 0x000000FF00000000;
        UInt64 emptySquareRank6 = nortOne(emptySquare & rank5) & emptySquare;
        return bPawnsSinglePushSource(bPawn ,emptySquareRank6);
    }

    int castle_right;

    UInt64 enpassant_bitboard = empty;
    int enpassant_target = -1;

    public UInt64 legal_king_move(int square, enumSide side, UInt64 occupancy)
    {
        UInt64 legal_king_move = 0;

        UInt64 mask = (side == enumSide.white) ? king_attack[square] & ~wPiece_bb : king_attack[square] & ~bPiece_bb;
        int[] pseudo_move_arr = get_bits_index_array(mask);

        int fail = 0;
        if (side == enumSide.white)
        {
            foreach (int pseudo_move in pseudo_move_arr)
            {
                //knight attack
                if (!(knight_attack[pseudo_move] & pieces_bb[(int)enumPiece.n]).Equals(empty)) fail = 1;
                //rook attack
                if (!((get_rook_attacks(pseudo_move, occupancy)) & pieces_bb[(int)enumPiece.r]).Equals(empty)) fail = 1;
                //bishop attack
                if (!((get_bishop_attacks(pseudo_move, occupancy)) & pieces_bb[(int)enumPiece.b]).Equals(empty)) fail = 1;
                //queen attack
                if (!((get_queen_attacks(pseudo_move, occupancy)) & pieces_bb[(int)enumPiece.q]).Equals(empty)) fail = 1;
                //pawn attack
                if (!(pawn_attack[(int)enumSide.white, pseudo_move] & pieces_bb[(int)enumPiece.p]).Equals(empty)) fail = 1;
                //king attack
                if (!(king_attack[pseudo_move] & pieces_bb[(int)enumPiece.k]).Equals(empty)) fail = 1;

                if (fail == 0) legal_king_move |= (UInt64)1 << pseudo_move;
                fail = 0;
            }
            if (!(((uint)castle_right & (uint)enumCastle.wk) == 0))
            {
                if ((occupancy & (UInt64)0x60).Equals(empty))
                {
                    if (is_square_attacked((int)enumSquare.e1, enumSide.black).Equals(empty) && is_square_attacked((int)enumSquare.f1, enumSide.black).Equals(empty) && is_square_attacked((int)enumSquare.g1, enumSide.black).Equals(empty))
                    {
                        legal_king_move |= set_bit(empty, (int)enumSquare.g1);
                    }
                }
            }
            if (!(((uint)castle_right & (uint)enumCastle.wq) == 0))
            {
                if ((occupancy & (UInt64)0xE).Equals(empty))
                {
                    if (is_square_attacked((int)enumSquare.e1, enumSide.black).Equals(empty) && is_square_attacked((int)enumSquare.d1, enumSide.black).Equals(empty) && is_square_attacked((int)enumSquare.c1, enumSide.black).Equals(empty))
                    {
                        legal_king_move |= set_bit(empty, (int)enumSquare.c1);
                    }
                }
            }
        } else {
            foreach (int pseudo_move in pseudo_move_arr)
            {
                //knight attack
                if (!(knight_attack[pseudo_move] & pieces_bb[(int)enumPiece.N]).Equals(empty)) fail = 1;
                //rook attack
                if (!((get_rook_attacks(pseudo_move, occupancy)) & pieces_bb[(int)enumPiece.R]).Equals(empty)) fail = 1;
                //bishop attack
                if (!((get_bishop_attacks(pseudo_move, occupancy)) & pieces_bb[(int)enumPiece.B]).Equals(empty)) fail = 1;
                //queen attack
                if (!((get_queen_attacks(pseudo_move, occupancy)) & pieces_bb[(int)enumPiece.Q]).Equals(empty)) fail = 1;
                //pawn attack
                if (!(pawn_attack[(int)enumSide.black, pseudo_move] & pieces_bb[(int)enumPiece.P]).Equals(empty)) fail = 1;
                //king attack
                if (!(king_attack[pseudo_move] & pieces_bb[(int)enumPiece.K]).Equals(empty)) fail = 1;

                if (fail == 0) legal_king_move |= (UInt64)1 << pseudo_move;
                fail = 0;
            }
            if (!(((uint)castle_right & (uint)enumCastle.bk) == 0))
            {
                if ((occupancy & (UInt64)0x06000000000000000).Equals(empty))
                {
                    if (is_square_attacked((int)enumSquare.e8, enumSide.white).Equals(empty) && is_square_attacked((int)enumSquare.f8, enumSide.white).Equals(empty) && is_square_attacked((int)enumSquare.g8, enumSide.white).Equals(empty))
                    {
                        legal_king_move |= set_bit(empty, (int)enumSquare.g8);
                    }
                }
            }
            if (!(((uint)castle_right & (uint)enumCastle.bq) == 0))
            {
                if ((occupancy & (UInt64)0xE00000000000000).Equals(empty))
                {
                    if (is_square_attacked((int)enumSquare.e8, enumSide.white).Equals(empty) && is_square_attacked((int)enumSquare.d8, enumSide.white).Equals(empty) && is_square_attacked((int)enumSquare.c8, enumSide.white).Equals(empty))
                    {
                        legal_king_move |= set_bit(empty, (int)enumSquare.c8);
                    }
                }
            }
        }
        return legal_king_move;
    }

    public UInt64 gen_pin_bitboard(int square, enumSide side, UInt64 occupancy)
    {
        //square = king position
        UInt64 pin_bitboard = 0;

        if (side == enumSide.white)
        {
            //rook pin 
            UInt64 attacking_rook = get_rook_attacks(square, bPiece_bb) & pieces_bb[(int)enumPiece.r];
            foreach (int rook_square in get_bits_index_array(attacking_rook))
            {
                pin_bitboard |= get_rook_attacks(rook_square, occupancy) & get_rook_attacks(square, occupancy) & wPiece_bb;
            }
            //bishop pin
            UInt64 attacking_bishop = get_bishop_attacks(square, bPiece_bb) & pieces_bb[(int)enumPiece.b];
            foreach (int bishop_square in get_bits_index_array(attacking_bishop))
            {
                pin_bitboard |= get_bishop_attacks(bishop_square, occupancy) & get_bishop_attacks(square, occupancy) & wPiece_bb;
            }
            //queen pin
            UInt64 attacking_queen = get_queen_attacks(square, bPiece_bb) & pieces_bb[(int)enumPiece.q];
            foreach (int queen_square in get_bits_index_array(attacking_queen))
            {
                pin_bitboard |= get_queen_attacks(queen_square, occupancy) & get_queen_attacks(square, occupancy) & wPiece_bb;
            }
        } else {
            //rook pin 
            UInt64 attacking_rook = get_rook_attacks(square, wPiece_bb) & pieces_bb[(int)enumPiece.R];
            foreach (int rook_square in get_bits_index_array(attacking_rook))
            {
                pin_bitboard |= get_rook_attacks(rook_square, occupancy) & get_rook_attacks(square, occupancy) & bPiece_bb;
            }
            //bishop pin
            UInt64 attacking_bishop = get_bishop_attacks(square, wPiece_bb) & pieces_bb[(int)enumPiece.B];
            foreach (int bishop_square in get_bits_index_array(attacking_bishop))
            {
                pin_bitboard |= get_bishop_attacks(bishop_square, occupancy) & get_bishop_attacks(square, occupancy) & bPiece_bb;
            }
            //queen pin
            UInt64 attacking_queen = get_queen_attacks(square, wPiece_bb) & pieces_bb[(int)enumPiece.Q];
            foreach (int queen_square in get_bits_index_array(attacking_queen))
            {
                pin_bitboard |= get_queen_attacks(queen_square, occupancy) & get_queen_attacks(square, occupancy) & bPiece_bb;
            }
        }
        return pin_bitboard;
    }

    public UInt64 draw_bit_ray(int from, int to)
    {
        UInt64 ray = 0;
        UInt64 mask = 0;
        UInt64 pieces = ((UInt64)1 << from) | ((UInt64)1 << to);
        
        for (int i = 0; i < 8; i++)
        {
            if (count_bits(pieces & (rank1 << 8 * i)) == 2) {ray |= (rank1 << 8 * i); break;}
            if (count_bits(pieces & (a_file << i)) == 2) {ray |= (a_file << i); break;}
        }
        for (int i = 0; i < 7; i++)
        {
            if (count_bits(pieces & (a1_h8_diag >> 8 * i)) == 2) {ray |= (a1_h8_diag >> 8 * i); break;}
            if (count_bits(pieces & (a1_h8_diag << 8 * i)) == 2) {ray |= (a1_h8_diag << 8 * i); break;}

            if (count_bits(pieces & (h1_a8_diag >> 8 * i)) == 2) {ray |= (h1_a8_diag >> 8 * i); break;}
            if (count_bits(pieces & (h1_a8_diag << 8 * i)) == 2) {ray |= (h1_a8_diag << 8 * i); break;}
        }

        if (from < to)
        {
            for (int i = from; i < to; i++)
            {
                mask |= set_bit(mask, i);
            }
        } else {
            for (int i = to; i < from; i++)
            {
                mask |= set_bit(mask, i);
            }
        }

        return ray & mask;
    }

    move_list[,] move_List = new move_list[12, 8];

    public move_list[,] gen_legal_move(int side)
    {
        int i;

        int[,] source_square = new int[12, 8];
        UInt64[,] legal_move = new UInt64[12, 8];
        move_List = new move_list[12, 8];

        UInt64 occupancy = wPiece_bb | bPiece_bb;
        UInt64 pin_bitboard;
        
        UInt64 capture_mask = 0xffffffffffffffff;
        UInt64 push_mask = 0xffffffffffffffff;

        UInt64 checkers_mask = (UInt64)0;
        int checkers_num;

        if (side == (int)enumSide.white)
        {
            checkers_mask = is_in_check(enumSide.white);
            checkers_num = count_bits(checkers_mask);

            move_List[(int)enumPiece.K, 0].source = ls1b_index(pieces_bb[(int)enumPiece.K]);
            move_List[(int)enumPiece.K, 0].target_bb = legal_king_move(ls1b_index(pieces_bb[(int)enumPiece.K]), (enumSide)side, occupancy);

            if (checkers_num > 1)
                return move_List;
            else
            {
                pin_bitboard = gen_pin_bitboard(ls1b_index(pieces_bb[(int)enumPiece.K]), (enumSide)side, occupancy);
                //knight
                int[] moveable_knights_index = get_bits_index_array(pieces_bb[(int)enumPiece.N] & ~pin_bitboard);
                for (i = 0; i < moveable_knights_index.Count(); i++)
                {
                    move_List[(int)enumPiece.N, i].source = moveable_knights_index[i];
                    move_List[(int)enumPiece.N, i].target_bb |= knight_attack[moveable_knights_index[i]] & bPiece_bb;
                    move_List[(int)enumPiece.N, i].target_bb |= knight_attack[moveable_knights_index[i]] & ~occupancy;
                }

                //rook
                int[] rooks_index = get_bits_index_array(pieces_bb[(int)enumPiece.R]);
                int[] pin_rooks_index = get_bits_index_array(pieces_bb[(int)enumPiece.R] & pin_bitboard);

                for (i = 0; i < rooks_index.Count(); i++)
                {
                    if (pin_rooks_index.Contains(rooks_index[i])){
                        move_List[(int)enumPiece.R, i].source = rooks_index[i];
                        move_List[(int)enumPiece.R, i].target_bb = get_rook_attacks(rooks_index[i], occupancy) & draw_bit_ray(rooks_index[i], ls1b_index(pieces_bb[(int)enumPiece.K]));
                        move_List[(int)enumPiece.R, i].target_bb &= ~wPiece_bb;
                    } else {
                        move_List[(int)enumPiece.R, i].source = rooks_index[i];
                        move_List[(int)enumPiece.R, i].target_bb = get_rook_attacks(rooks_index[i], occupancy);
                        move_List[(int)enumPiece.R, i].target_bb &= ~wPiece_bb;
                    }
                }

                //bishop
                int[] bishops_index = get_bits_index_array(pieces_bb[(int)enumPiece.B]);
                int[] pin_bishops_index = get_bits_index_array(pieces_bb[(int)enumPiece.B] & pin_bitboard);
                for (i = 0; i < bishops_index.Count(); i++)
                {
                    if (pin_bishops_index.Contains(bishops_index[i])){
                        move_List[(int)enumPiece.B, i].source = bishops_index[i];
                        move_List[(int)enumPiece.B, i].target_bb = get_bishop_attacks(bishops_index[i], occupancy) & draw_bit_ray(bishops_index[i], ls1b_index(pieces_bb[(int)enumPiece.K]));
                        move_List[(int)enumPiece.B, i].target_bb &= ~wPiece_bb;
                    } else {
                        move_List[(int)enumPiece.B, i].source = bishops_index[i];
                        move_List[(int)enumPiece.B, i].target_bb = get_bishop_attacks(bishops_index[i], occupancy);
                        move_List[(int)enumPiece.B, i].target_bb &= ~wPiece_bb;
                    }
                }

                //queen
                int queen_index = ls1b_index(pieces_bb[(int)enumPiece.Q]);
                if ((pieces_bb[(int)enumPiece.Q] & pin_bitboard).Equals(empty))
                {
                    move_List[(int)enumPiece.Q, 0].source = queen_index;
                    move_List[(int)enumPiece.Q, 0].target_bb = get_queen_attacks(queen_index, occupancy);
                    move_List[(int)enumPiece.Q, 0].target_bb &= ~wPiece_bb;
                } else {
                    move_List[(int)enumPiece.Q, 0].source = queen_index;
                    move_List[(int)enumPiece.Q, 0].target_bb = get_queen_attacks(queen_index, occupancy) & draw_bit_ray(queen_index, ls1b_index(pieces_bb[(int)enumPiece.K]));
                    move_List[(int)enumPiece.Q, 0].target_bb &= ~wPiece_bb;
                }

                //pawn
                int[] pawns_index = get_bits_index_array(pieces_bb[(int)enumPiece.P]);
                int[] push_pawns_index = get_bits_index_array(wPawnsSinglePushSource(pieces_bb[(int)enumPiece.P], ~occupancy));
                int[] pin_pawns_index = get_bits_index_array(wPawnsSinglePushSource(pieces_bb[(int)enumPiece.P], ~occupancy) & pin_bitboard);

                for (i = 0; i < pawns_index.Count(); i++)
                {
                    if (pin_pawns_index.Contains(pawns_index[i]))
                    {
                        move_List[(int)enumPiece.P, i].source = pawns_index[i];
                        move_List[(int)enumPiece.P, i].target_bb = pawn_attack[(int)enumSide.white, pawns_index[i]] & draw_bit_ray(pawns_index[i], ls1b_index(pieces_bb[(int)enumPiece.K])) & bPiece_bb;
                        move_List[(int)enumPiece.P, i].target_bb |= wPawnsSinglePushTarget(pieces_bb[(int)enumPiece.P] & pin_bitboard, ~occupancy) & draw_bit_ray(pawns_index[i], ls1b_index(pieces_bb[(int)enumPiece.K])) | wPawnsDoublePushTarget(pieces_bb[(int)enumPiece.P] & pin_bitboard, ~occupancy) & draw_bit_ray(pawns_index[i], ls1b_index(pieces_bb[(int)enumPiece.K]));
                        if (!enpassant_bitboard.Equals(empty))
                        {
                            if(!(pawn_attack[(int)enumSide.white, pawns_index[i]] & draw_bit_ray(pawns_index[i], ls1b_index(pieces_bb[(int)enumPiece.K])) & bPiece_bb).Equals(empty))
                            {
                                move_List[(int)enumPiece.P, i].target_bb |= pawn_attack[(int)enumSide.white, pawns_index[i]] & draw_bit_ray(pawns_index[i], ls1b_index(pieces_bb[(int)enumPiece.K])) & enpassant_bitboard;
                                move_List[(int)enumPiece.P, i].flag = "enpassant";
                            }
                        }
                    } else {
                        move_List[(int)enumPiece.P, i].source = pawns_index[i];
                        move_List[(int)enumPiece.P, i].target_bb = pawn_attack[(int)enumSide.white, pawns_index[i]] & bPiece_bb;
                        move_List[(int)enumPiece.P, i].target_bb |= wPawnsSinglePushTarget(pieces_bb[(int)enumPiece.P], ~occupancy) & nortOne((UInt64)1 << pawns_index[i]);
                        move_List[(int)enumPiece.P, i].target_bb |= wPawnsDoublePushTarget(pieces_bb[(int)enumPiece.P], ~occupancy) & nortOne(nortOne((UInt64)1 << pawns_index[i]));
                        if (!enpassant_bitboard.Equals(empty))
                        {
                            if(!(pawn_attack[(int)enumSide.white, pawns_index[i]] & enpassant_bitboard).Equals(empty))
                            {
                                move_List[(int)enumPiece.P, i].target_bb |= pawn_attack[(int)enumSide.white, pawns_index[i]] & enpassant_bitboard;
                                move_List[(int)enumPiece.P, i].flag = "enpassant";
                            }
                        }
                    }
                }

                if (checkers_num == 1)
                {
                    capture_mask = checkers_mask;
                    if (!(checkers_mask & pieces_bb[(int)enumPiece.b]).Equals(empty) || !(checkers_mask & pieces_bb[(int)enumPiece.r]).Equals(empty) || !(checkers_mask & pieces_bb[(int)enumPiece.q]).Equals(empty))
                    {
                        push_mask = draw_bit_ray(ls1b_index(pieces_bb[(int)enumPiece.K]), ls1b_index(checkers_mask));
                    } else {
                        push_mask = empty;
                    }
                    for (int j = 0; j < move_List.GetLength(0); j++)
                    {
                        for (int k = 0; k < move_List.GetLength(1); k++)
                        {
                            if (move_List[j, k].source != move_List[(int)enumPiece.K, 0].source)
                                move_List[j, k].target_bb &= capture_mask | push_mask;
                            else move_List[j, k].target_bb |= move_List[j, k].target_bb & capture_mask;
                        }
                    }
                }
                for (int j = 0; j < move_List.GetLength(0); j++)
                {
                    for (int k = 0; k < move_List.GetLength(1); k++)
                    {
                            move_List[j, k].target_array = get_bits_index_array(move_List[j, k].target_bb);
                    }
                }
            }
            return move_List;
        } else {
            checkers_mask = is_in_check(enumSide.black);
            checkers_num = count_bits(checkers_mask);

            move_List[(int)enumPiece.k, 0].source = ls1b_index(pieces_bb[(int)enumPiece.k]);
            move_List[(int)enumPiece.k, 0].target_bb = legal_king_move(ls1b_index(pieces_bb[(int)enumPiece.k]), (enumSide)side, occupancy);

            if (checkers_num > 1)
                return move_List;
            else
            {
                pin_bitboard = gen_pin_bitboard(ls1b_index(pieces_bb[(int)enumPiece.k]), (enumSide)side, occupancy);
                //knight
                int[] moveable_knights_index = get_bits_index_array(pieces_bb[(int)enumPiece.n] & ~pin_bitboard);
                for (i = 0; i < moveable_knights_index.Count(); i++)
                {
                    move_List[(int)enumPiece.n, i].source = moveable_knights_index[i];
                    move_List[(int)enumPiece.n, i].target_bb |= knight_attack[moveable_knights_index[i]] & wPiece_bb;
                    move_List[(int)enumPiece.n, i].target_bb |= knight_attack[moveable_knights_index[i]] & ~occupancy;
                }

                //rook
                int[] rooks_index = get_bits_index_array(pieces_bb[(int)enumPiece.r]);
                int[] pin_rooks_index = get_bits_index_array(pieces_bb[(int)enumPiece.r] & pin_bitboard);

                for (i = 0; i < rooks_index.Count(); i++)
                {
                    if (pin_rooks_index.Contains(rooks_index[i])){
                        move_List[(int)enumPiece.r, i].source = rooks_index[i];
                        move_List[(int)enumPiece.r, i].target_bb = get_rook_attacks(rooks_index[i], occupancy) & draw_bit_ray(rooks_index[i], ls1b_index(pieces_bb[(int)enumPiece.k]));
                        move_List[(int)enumPiece.r, i].target_bb &= ~bPiece_bb;
                    } else {
                        move_List[(int)enumPiece.r, i].source = rooks_index[i];
                        move_List[(int)enumPiece.r, i].target_bb = get_rook_attacks(rooks_index[i], occupancy);
                        move_List[(int)enumPiece.r, i].target_bb &= ~bPiece_bb;
                    }
                }

                //bishop
                int[] bishops_index = get_bits_index_array(pieces_bb[(int)enumPiece.b]);
                int[] pin_bishops_index = get_bits_index_array(pieces_bb[(int)enumPiece.b] & pin_bitboard);
                for (i = 0; i < bishops_index.Count(); i++)
                {
                    if (pin_bishops_index.Contains(bishops_index[i])){
                        move_List[(int)enumPiece.b, i].source = bishops_index[i];
                        move_List[(int)enumPiece.b, i].target_bb = get_bishop_attacks(bishops_index[i], occupancy) & draw_bit_ray(bishops_index[i], ls1b_index(pieces_bb[(int)enumPiece.k]));
                        move_List[(int)enumPiece.b, i].target_bb &= ~bPiece_bb;
                    } else {
                        move_List[(int)enumPiece.b, i].source = bishops_index[i];
                        move_List[(int)enumPiece.b, i].target_bb = get_bishop_attacks(bishops_index[i], occupancy);
                        move_List[(int)enumPiece.b, i].target_bb &= ~bPiece_bb;
                    }
                }

                //queen
                int queen_index = ls1b_index(pieces_bb[(int)enumPiece.q]);
                if ((pieces_bb[(int)enumPiece.q] & pin_bitboard).Equals(empty))
                {
                    move_List[(int)enumPiece.q, 0].source = queen_index;
                    move_List[(int)enumPiece.q, 0].target_bb = get_queen_attacks(queen_index, occupancy);
                    move_List[(int)enumPiece.q, 0].target_bb &= ~bPiece_bb;
                } else {
                    move_List[(int)enumPiece.q, 0].source = queen_index;
                    move_List[(int)enumPiece.q, 0].target_bb = get_queen_attacks(queen_index, occupancy) & draw_bit_ray(queen_index, ls1b_index(pieces_bb[(int)enumPiece.k]));
                    move_List[(int)enumPiece.q, 0].target_bb &= ~bPiece_bb;
                }

                //pawn
                int[] pawns_index = get_bits_index_array(pieces_bb[(int)enumPiece.p]);
                int[] push_pawns_index = get_bits_index_array(bPawnsSinglePushSource(pieces_bb[(int)enumPiece.p], ~occupancy));
                int[] pin_pawns_index = get_bits_index_array(bPawnsSinglePushSource(pieces_bb[(int)enumPiece.p], ~occupancy) & pin_bitboard);

                for (i = 0; i < pawns_index.Count(); i++)
                {
                    if (pin_pawns_index.Contains(pawns_index[i]))
                    {
                        move_List[(int)enumPiece.p, i].source = pawns_index[i];
                        move_List[(int)enumPiece.p, i].target_bb = pawn_attack[(int)enumSide.black, pawns_index[i]] & draw_bit_ray(pawns_index[i], ls1b_index(pieces_bb[(int)enumPiece.k])) & wPiece_bb;
                        move_List[(int)enumPiece.p, i].target_bb |= bPawnsSinglePushTarget(pieces_bb[(int)enumPiece.p] & pin_bitboard, ~occupancy) & draw_bit_ray(pawns_index[i], ls1b_index(pieces_bb[(int)enumPiece.k])) | bPawnsDoublePushTarget(pieces_bb[(int)enumPiece.p] & pin_bitboard, ~occupancy) & draw_bit_ray(pawns_index[i], ls1b_index(pieces_bb[(int)enumPiece.k]));
                        if (!enpassant_bitboard.Equals(empty))
                        {
                            if(!(pawn_attack[(int)enumSide.black, pawns_index[i]] & draw_bit_ray(pawns_index[i], ls1b_index(pieces_bb[(int)enumPiece.k])) & bPiece_bb).Equals(empty))
                            {
                                move_List[(int)enumPiece.p, i].target_bb |= pawn_attack[(int)enumSide.black, pawns_index[i]] & draw_bit_ray(pawns_index[i], ls1b_index(pieces_bb[(int)enumPiece.k])) & enpassant_bitboard;
                                move_List[(int)enumPiece.p, i].flag = "enpassant";
                            }
                        }
                    } else {
                        move_List[(int)enumPiece.p, i].source = pawns_index[i];
                        move_List[(int)enumPiece.p, i].target_bb = pawn_attack[(int)enumSide.black, pawns_index[i]] & wPiece_bb;
                        move_List[(int)enumPiece.p, i].target_bb |= bPawnsSinglePushTarget(pieces_bb[(int)enumPiece.p], ~occupancy) & soutOne((UInt64)1 << pawns_index[i]);
                        move_List[(int)enumPiece.p, i].target_bb |= bPawnsDoublePushTarget(pieces_bb[(int)enumPiece.p], ~occupancy) & soutOne(soutOne((UInt64)1 << pawns_index[i]));
                        if (!enpassant_bitboard.Equals(empty))
                        {
                            if(!(pawn_attack[(int)enumSide.black, pawns_index[i]] & enpassant_bitboard).Equals(empty))
                            {
                                move_List[(int)enumPiece.p, i].target_bb |= pawn_attack[(int)enumSide.black, pawns_index[i]] & enpassant_bitboard;
                                move_List[(int)enumPiece.p, i].flag = "enpassant";
                            }
                        }
                    }
                }

                if (checkers_num == 1)
                {
                    capture_mask = checkers_mask;
                    if (!(checkers_mask & pieces_bb[(int)enumPiece.B]).Equals(empty) || !(checkers_mask & pieces_bb[(int)enumPiece.R]).Equals(empty) || !(checkers_mask & pieces_bb[(int)enumPiece.Q]).Equals(empty))
                    {
                        push_mask = draw_bit_ray(ls1b_index(pieces_bb[(int)enumPiece.k]), ls1b_index(checkers_mask));
                    } else {
                        push_mask = empty;
                    }
                    for (int j = 0; j < move_List.GetLength(0); j++)
                    {
                        for (int k = 0; k < move_List.GetLength(1); k++)
                        {
                            if (move_List[j, k].source != move_List[(int)enumPiece.k, 0].source)
                                move_List[j, k].target_bb &= capture_mask | push_mask;
                            else move_List[j, k].target_bb |= move_List[j, k].target_bb & capture_mask;
                        }
                    }
                }
                for (int j = 0; j < move_List.GetLength(0); j++)
                {
                    for (int k = 0; k < move_List.GetLength(1); k++)
                    {
                            move_List[j, k].target_array = get_bits_index_array(move_List[j, k].target_bb);
                    }
                }
            }
        }
        return move_List;
    }

    public void showLegalMove(Node2D piece)
    {
        int square = pos_to_square(this.WorldToMap(piece.GlobalPosition));
        TileMap overlay = (TileMap)this.GetNode("Overlay");

        for (int j = 0; j < move_List.GetLength(1); j++)
        {
            if (move_List[get_piece_type(square), j].source == square)
            {
                foreach(int tile in move_List[get_piece_type(square), j].target_array)
                {
                    overlay.SetCellv(square_to_pos(tile), 0);
                }
            }
        }
    }

    UInt64 save_wPiece_bb, save_bPiece_bb;
    UInt64[] save_pieces_bb = new UInt64[12];

    public void save_boardstate()
    {
        save_wPiece_bb = wPiece_bb;
        save_bPiece_bb = bPiece_bb;
        for(int i = 0; i < 12; i++)
        {
            save_pieces_bb[i] = pieces_bb[i];
        }
    }

    public void load_boardstate(UInt64[] bb)
    {
        wPiece_bb = bb[0];
        bPiece_bb = bb[1];
        for(int i = 2; i < 14; i++)
        {
            pieces_bb[i - 2] = bb[i];
        }
        enpassant_bitboard = bb[14];
        enpassant_target = ls1b_index(bb[15]);
        castle_right = (int)bb[16];
    }

    public void check_endgame(int side)
    {
        for (int i = 0; i < 12; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (!move_List[i, j].is_empty()) goto LoopEnd;
            }
        }
        
        if (!is_in_check((enumSide)side).Equals(empty))
        {
            main.Call("showEndGame", side, "checkmate");
        } else {
            main.Call("showEndGame", side, "stalemate");
        }
        LoopEnd: return;
    }

    public bool make_move(Node2D piece, int target)
    {
        if (piece == null) return false;
        bool capture = false;
        bool check = false;
        int source = pos_to_square(this.WorldToMap(piece.GlobalPosition));

        if (piece.IsInGroup("pawns"))
        {
            if (target == ls1b_index(enpassant_bitboard))
            {
                for (int i = 0; i < 8; i++)
                {
                    if (move_List[get_piece_type(source), i].target_array.Contains(target) && move_List[get_piece_type(source), i].flag == "enpassant")
                    {
                        enpassant(piece, source, target);
                        capture = true;
                        if(!(is_in_check((get_piece_type(source) <= 5) ? enumSide.black : enumSide.white).Equals(empty))) check = true;
                        if (check)
                        {
                            main.Call("play_sound", "check");
                        } else {
                            main.Call("play_sound", "capture");
                        }
                        return true;
                    }
                }
            }
        }

        if (piece.IsInGroup("kings"))
        {
            bool castle_move = false;

            if (target == (int)enumSquare.g1 && move_List[(int)enumPiece.K, 0].target_array.Contains(target) && move_List[(int)enumPiece.K, 0].source == (int)enumSquare.e1)
            {
                castle((int)enumCastle.wk);
                castle_move = true;
            }
            if (target == (int)enumSquare.c1 && move_List[(int)enumPiece.K, 0].target_array.Contains(target) && move_List[(int)enumPiece.K, 0].source == (int)enumSquare.e1)
            {
                castle((int)enumCastle.wq);
                castle_move = true;
            }
            if (target == (int)enumSquare.g8 && move_List[(int)enumPiece.k, 0].target_array.Contains(target) && move_List[(int)enumPiece.k, 0].source == (int)enumSquare.e8)
            {
                castle((int)enumCastle.bk);
                castle_move = true;
            }
            if (target == (int)enumSquare.c8 && move_List[(int)enumPiece.k, 0].target_array.Contains(target) && move_List[(int)enumPiece.k, 0].source == (int)enumSquare.e8)
            {
                castle((int)enumCastle.bq);
                castle_move = true;
            }

            if(!(is_in_check((get_piece_type(source) <= 5) ? enumSide.black : enumSide.white).Equals(empty))) check = true;
            if (castle_move) 
            {
                if (check)
                {
                    main.Call("play_sound", "check");
                    return true;
                } else {
                    main.Call("play_sound", "castle");
                    return true;
                }
            }
        }

        for (int i = 0; i < 8; i++)
        {
            if (move_List[get_piece_type(source), i].target_array.Contains(target) && move_List[get_piece_type(source), i].source == source) goto LoopEnd;
        }
        return false;

        LoopEnd: UInt64 occupancy = wPiece_bb | bPiece_bb;
        if (!(((UInt64)1 << target) & occupancy).Equals(empty))
        {
            main.Call("deletePiece", square_to_pos(target));
            capture = true;
            pieces_bb[get_piece_type(target)] = pieces_bb[get_piece_type(target)] & ~((UInt64)1 << target);
        }
        piece.GlobalPosition = this.MapToWorld(square_to_pos(target)) + this.CellSize * 2 / 4;
        pieces_bb[get_piece_type(source)] = set_bit(pieces_bb[get_piece_type(source)] & ~((UInt64)1 << source), target);
        if(!(is_in_check((get_piece_type(target) <= 5) ? enumSide.black : enumSide.white).Equals(empty))) check = true;

        if (check)
        {
            main.Call("play_sound", "check");
        }
        else if (!check && capture)
        {
            main.Call("play_sound", "capture");
        } else {
            main.Call("play_sound", "move");
        }

            
        wPiece_bb = bPiece_bb = empty;
        for (int i = 0; i < 6; i++)
        {
            wPiece_bb |= pieces_bb[i];
        }
        for (int i = 6; i < 12; i++)
        {
            bPiece_bb |= pieces_bb[i];
        }
        enpassant_bitboard = empty;
        enpassant_target = -1;
        if(piece.IsInGroup("pawns"))
        {
            if(target == ls1b_index(nortOne(nortOne(set_bit(empty, source)))))
            {
                enpassant_bitboard = soutOne(set_bit(empty, target));
                enpassant_target = target;
            }
            if(target == ls1b_index(soutOne(soutOne(set_bit(empty, source)))))
            {
                enpassant_bitboard = nortOne(set_bit(empty, target));
                enpassant_target = target;
            }
        }
        if(piece.IsInGroup("kings"))
        {
            if (piece.IsInGroup("white_pieces")) castle_right &= 12;
            else castle_right &= 3;
        }
        if(piece.IsInGroup("rooks"))
        {
            switch(source)
            {
                case (int)enumSquare.h1: castle_right &= 14; break; //1110
                case (int)enumSquare.a1: castle_right &= 13; break; //1101
                case (int)enumSquare.h8: castle_right &= 11; break; //1011
                case (int)enumSquare.a8: castle_right &= 7; break; //0111
            }
        }
        return true;
    }

    public void enpassant(Node2D piece ,int source, int target)
    {
        pieces_bb[get_piece_type(enpassant_target)] &= ~((UInt64)1 << enpassant_target);
        main.Call("deletePiece", square_to_pos(enpassant_target));
        piece.GlobalPosition = this.MapToWorld(square_to_pos(target)) + this.CellSize * 2 / 4;

        enpassant_bitboard = 0;
        enpassant_target = -1;

        pieces_bb[get_piece_type(source)] = set_bit(pieces_bb[get_piece_type(source)] & ~((UInt64)1 << source), target);
        wPiece_bb = bPiece_bb = empty;
        for (int i = 0; i < 6; i++)
        {
            wPiece_bb |= pieces_bb[i];
        }
        for (int i = 6; i < 12; i++)
        {
            bPiece_bb |= pieces_bb[i];
        }
    }

    public void castle(int castle)
    {
        if (castle == 1)
        {
            //white short castle
            Node2D king = (Node2D)GetNode("/root/Main/king_white");
            Node2D rook = (Node2D)GetNode("/root/Main/rook_white_2");
            pieces_bb[(int)enumPiece.K] = set_bit(empty, (int)enumSquare.g1);
            pieces_bb[(int)enumPiece.R] = set_bit((pieces_bb[(int)enumPiece.R] & ((UInt64)1 << (int)enumSquare.h1)), (int)enumSquare.f1);
            king.GlobalPosition = this.MapToWorld(square_to_pos((int)enumSquare.g1)) + this.CellSize * 2 / 4;
            rook.GlobalPosition = this.MapToWorld(square_to_pos((int)enumSquare.f1)) + this.CellSize * 2 / 4;
        }
        if (castle == 2)
        {
            //white long castle
            Node2D king = (Node2D)GetNode("/root/Main/king_white");
            Node2D rook = (Node2D)GetNode("/root/Main/rook_white_1");
            pieces_bb[(int)enumPiece.K] = set_bit(empty, (int)enumSquare.c1);
            pieces_bb[(int)enumPiece.R] = set_bit((pieces_bb[(int)enumPiece.R] & ((UInt64)1 << (int)enumSquare.a1)), (int)enumSquare.d1);
            king.GlobalPosition = this.MapToWorld(square_to_pos((int)enumSquare.c1)) + this.CellSize * 2 / 4;
            rook.GlobalPosition = this.MapToWorld(square_to_pos((int)enumSquare.d1)) + this.CellSize * 2 / 4;
        }
        if (castle == 4)
        {
            //black short castle
            Node2D king = (Node2D)GetNode("/root/Main/king_black");
            Node2D rook = (Node2D)GetNode("/root/Main/rook_black_2");
            pieces_bb[(int)enumPiece.k] = set_bit(empty, (int)enumSquare.g8);
            pieces_bb[(int)enumPiece.r] = set_bit((pieces_bb[(int)enumPiece.r] & ((UInt64)1 << (int)enumSquare.h8)), (int)enumSquare.f8);
            king.GlobalPosition = this.MapToWorld(square_to_pos((int)enumSquare.g8)) + this.CellSize * 2 / 4;
            rook.GlobalPosition = this.MapToWorld(square_to_pos((int)enumSquare.f8)) + this.CellSize * 2 / 4;
        }
        if (castle == 8)
        {
            //black long castle
            Node2D king = (Node2D)GetNode("/root/Main/king_black");
            Node2D rook = (Node2D)GetNode("/root/Main/rook_black_1");
            pieces_bb[(int)enumPiece.k] = set_bit(empty, (int)enumSquare.c8);
            pieces_bb[(int)enumPiece.r] = set_bit((pieces_bb[(int)enumPiece.r] & ((UInt64)1 << (int)enumSquare.a8)), (int)enumSquare.d8);
            king.GlobalPosition = this.MapToWorld(square_to_pos((int)enumSquare.c8)) + this.CellSize * 2 / 4;
            rook.GlobalPosition = this.MapToWorld(square_to_pos((int)enumSquare.d8)) + this.CellSize * 2 / 4;
        }

        main.Call("play_sound", "castle");

        wPiece_bb = bPiece_bb = empty;
        for (int i = 0; i < 6; i++)
        {
            wPiece_bb |= pieces_bb[i];
        }
        for (int i = 6; i < 12; i++)
        {
            bPiece_bb |= pieces_bb[i];
        }
    }

    public struct move_list
    {
        public move_list(int x, int[] y, UInt64 bb, string sflag)
        {
            source = x;
            target_array = y;
            target_bb = bb;
            flag = sflag;
        }

        public int source {get; set;}
        public int[] target_array {get; set;}
        public UInt64 target_bb {get; set;}
        public string flag {get; set;}

        public bool is_empty()
        {
            if (target_bb.Equals(empty)) return true;
            return false;
        }
    }

    public void enpassant_perft(int source, int target)
    {
        pieces_bb[get_piece_type(enpassant_target)] &= ~((UInt64)1 << enpassant_target);

        enpassant_bitboard = 0;
        enpassant_target = -1;

        pieces_bb[get_piece_type(source)] = set_bit(pieces_bb[get_piece_type(source)] & ~((UInt64)1 << source), target);
        wPiece_bb = bPiece_bb = empty;
        for (int i = 0; i < 6; i++)
        {
            wPiece_bb |= pieces_bb[i];
        }
        for (int i = 6; i < 12; i++)
        {
            bPiece_bb |= pieces_bb[i];
        }
    }

    public void castle_perft(int castle)
    {
        if (castle == 1)
        {
            //white short castle
            pieces_bb[(int)enumPiece.K] = set_bit(empty, (int)enumSquare.g1);
            pieces_bb[(int)enumPiece.R] = set_bit(empty, (int)enumSquare.f1);
        }
        if (castle == 2)
        {
           //white long castle
            pieces_bb[(int)enumPiece.K] = set_bit(empty, (int)enumSquare.c1);
            pieces_bb[(int)enumPiece.R] = set_bit(empty, (int)enumSquare.d1);
        }
        if (castle == 4)
        {
            //black short castle
            pieces_bb[(int)enumPiece.k] = set_bit(empty, (int)enumSquare.g8);
            pieces_bb[(int)enumPiece.r] = set_bit(empty, (int)enumSquare.f8);
        }
        if (castle == 8)
        {
            //black long castle
            pieces_bb[(int)enumPiece.k] = set_bit(empty, (int)enumSquare.c8);
            pieces_bb[(int)enumPiece.r] = set_bit(empty, (int)enumSquare.d8);
        }

        wPiece_bb = bPiece_bb = empty;
        for (int i = 0; i < 6; i++)
        {
            wPiece_bb |= pieces_bb[i];
        }
        for (int i = 6; i < 12; i++)
        {
            bPiece_bb |= pieces_bb[i];
        }
    }

    public int make_move_perft(int source, int target)
    {
        if (get_piece_type(source) == (int)enumPiece.P || get_piece_type(source) == (int)enumPiece.p)
        {
            if (target == ls1b_index(enpassant_bitboard))
            {
                for (int i = 0; i < 8; i++)
                {
                    if (move_List[get_piece_type(source), i].target_array.Contains(target) && move_List[get_piece_type(source), i].flag == "enpassant")
                    {
                        enpassant_perft(source, target);
                        return target;
                    }
                }
            }
        }

        if (get_piece_type(source) == (int)enumPiece.K || get_piece_type(source) == (int)enumPiece.k)
        {
            if (target == (int)enumSquare.g1 && move_List[(int)enumPiece.K, 0].target_array.Contains(target) && move_List[(int)enumPiece.K, 0].source == (int)enumSquare.e1)
            {
                castle_perft((int)enumCastle.wk);
                return target;
            }
            if (target == (int)enumSquare.c1 && move_List[(int)enumPiece.K, 0].target_array.Contains(target) && move_List[(int)enumPiece.K, 0].source == (int)enumSquare.e1)
            {
                castle_perft((int)enumCastle.wq);
                return target;
            }
            if (target == (int)enumSquare.g8 && move_List[(int)enumPiece.k, 0].target_array.Contains(target) && move_List[(int)enumPiece.k, 0].source == (int)enumSquare.e8)
            {
                castle_perft((int)enumCastle.bk);
                return target;
            }
            if (target == (int)enumSquare.c8 && move_List[(int)enumPiece.k, 0].target_array.Contains(target) && move_List[(int)enumPiece.k, 0].source == (int)enumSquare.e8)
            {
                castle_perft((int)enumCastle.bq);
                return target;
            }
        }

        UInt64 occupancy = wPiece_bb | bPiece_bb;
        if (!(((UInt64)1 << target) & occupancy).Equals(empty))
        {
            pieces_bb[get_piece_type(target)] = pieces_bb[get_piece_type(target)] & ~((UInt64)1 << target);
        }

        pieces_bb[get_piece_type(source)] = set_bit(pieces_bb[get_piece_type(source)] & ~((UInt64)1 << source), target);
        wPiece_bb = bPiece_bb = empty;
        for (int i = 0; i < 6; i++)
        {
            wPiece_bb |= pieces_bb[i];
        }
        for (int i = 6; i < 12; i++)
        {
            bPiece_bb |= pieces_bb[i];
        }
        enpassant_bitboard = empty;
        enpassant_target = -1;
        if(get_piece_type(source) == (int)enumPiece.P || get_piece_type(source) == (int)enumPiece.p)
        {
            if(target == ls1b_index(nortOne(nortOne(set_bit(empty, source)))))
            {
                enpassant_bitboard = soutOne(set_bit(empty, target));
                enpassant_target = target;
            }
            if(target == ls1b_index(soutOne(soutOne(set_bit(empty, source)))))
            {
                enpassant_bitboard = nortOne(set_bit(empty, target));
                enpassant_target = target;
            }
        }
        if(get_piece_type(source) == (int)enumPiece.K || get_piece_type(source) == (int)enumPiece.k)
        {
            if (get_piece_type(source) == (int)enumPiece.K) castle_right &= 12;
            else castle_right &= 3;
        }
        if(get_piece_type(source) == (int)enumPiece.R || get_piece_type(source) == (int)enumPiece.r)
        {
            switch(source)
            {
                case (int)enumSquare.h1: castle_right &= 14; break; //1110
                case (int)enumSquare.a1: castle_right &= 13; break; //1101
                case (int)enumSquare.h8: castle_right &= 11; break; //1011
                case (int)enumSquare.a8: castle_right &= 7; break; //0111
            }
        }
        return target;
    }

    public UInt64 perft(int depth, bool side)
    {
        UInt64 nodes = 0;
        int count = 0;
        UInt64[] bb = new UInt64[17];
        move_list[,] perft_move_Lists = new move_list[12, 8];

        save_boardstate();
        bb[0] = save_wPiece_bb;
        bb[1] = save_bPiece_bb;
        for (int i = 2; i < 14; i++)
        {
            bb[i] = save_pieces_bb[i - 2];
        }
        bb[14] = enpassant_bitboard;
        bb[15] = set_bit(empty, enpassant_target);
        bb[16] = (UInt64)castle_right;

        if (side) {
            perft_move_Lists = gen_legal_move((int)enumSide.white);
        } else {
            perft_move_Lists = gen_legal_move((int)enumSide.black);
        }

        if (depth == 1)
        {
            for (int i = 0; i < perft_move_Lists.GetLength(0); i++)
            {
                for (int j = 0; j < perft_move_Lists.GetLength(1); j++)
                {
                    if (perft_move_Lists[i, j].target_array.Count() != 0)
                    {
                        count += perft_move_Lists[i, j].target_array.Count();
                    }
                }
            }
            return (UInt64)count;
        }

        for (int i = 0; i < perft_move_Lists.GetLength(0); i++)
        {
            for (int j = 0; j < perft_move_Lists.GetLength(1); j++)
            {
                for (int k = 0; k < perft_move_Lists[i, j].target_array.Count(); k++)
                {
                    make_move_perft(perft_move_Lists[i, j].source, perft_move_Lists[i, j].target_array[k]);
                    nodes += perft(depth - 1, !side);
                    load_boardstate(bb);
                }
            }
        }

        return nodes;
    }

    Node2D main;
    public override void _Ready()
    {
        main = (Node2D)GetNode("/root/Main");
        init_all();

        pieces_bb[(int)enumPiece.K] = 0x0000000000000010;
        pieces_bb[(int)enumPiece.k] = 0x1000000000000000;
        pieces_bb[(int)enumPiece.Q] = 0x0000000000000008;
        pieces_bb[(int)enumPiece.q] = 0x0800000000000000;
        pieces_bb[(int)enumPiece.R] = 0x0000000000000081;
        pieces_bb[(int)enumPiece.r] = 0x8100000000000000;
        pieces_bb[(int)enumPiece.B] = 0x0000000000000024;
        pieces_bb[(int)enumPiece.b] = 0x2400000000000000;
        pieces_bb[(int)enumPiece.N] = 0x0000000000000042;
        pieces_bb[(int)enumPiece.n] = 0x4200000000000000;
        pieces_bb[(int)enumPiece.P] = 0x000000000000FF00;
        pieces_bb[(int)enumPiece.p] = 0x00FF000000000000;

        wPiece_bb = pieces_bb[(int)enumPiece.K] | pieces_bb[(int)enumPiece.Q] | pieces_bb[(int)enumPiece.N] | pieces_bb[(int)enumPiece.R] | pieces_bb[(int)enumPiece.B] | pieces_bb[(int)enumPiece.P];
        bPiece_bb = pieces_bb[(int)enumPiece.k] | pieces_bb[(int)enumPiece.q] | pieces_bb[(int)enumPiece.n] | pieces_bb[(int)enumPiece.r] | pieces_bb[(int)enumPiece.b] | pieces_bb[(int)enumPiece.p];

        castle_right = 15; //binary 1111: both side can castle king & queen side
        enpassant_target = -1;
        enpassant_bitboard = 0;

        var watch = new System.Diagnostics.Stopwatch();
            
        watch.Start();

        GD.Print(perft(4, true)); //perft(4) = 197,281 //perft(5) = 4,865,609 //perft(6) = 119,060,324

        watch.Stop();

        GD.Print(watch.ElapsedMilliseconds);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        
    }
}